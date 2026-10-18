using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using AIUsageWidget.Core;

namespace AIUsageWidget.Providers;

/// <summary>
/// Reads Codex limits through the official <c>codex app-server</c> JSON-RPC interface
/// (<c>account/rateLimits/read</c> and the <c>account/rateLimits/updated</c> notification).
/// The app-server uses the CLI's existing sign-in; this class never reads credentials.
/// </summary>
public sealed class CodexProvider : IUsageProvider
{
    private static readonly TimeSpan RestartDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MissingCliRetry = TimeSpan.FromMinutes(5);

    private readonly CancellationTokenSource _stop = new();
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonObject>> _pending = new();
    private readonly SemaphoreSlim _refresh = new(0);
    private readonly IntervalSetting _interval = new(TimeSpan.FromMinutes(5));
    private Process? _process;
    private long _nextId;
    private (UsageWindow? FiveHour, UsageWindow? Weekly) _windows;

    public ProviderId Id => ProviderId.Codex;

    public ProviderSnapshot Current { get; private set; } = ProviderSnapshot.Of(ProviderId.Codex, ProviderState.Starting);

    public event Action<ProviderSnapshot>? Changed;

    private bool _started;

    public void Start()
    {
        _started = true;
        _ = Task.Run(RunAsync);
    }

    public void SetInterval(TimeSpan? interval)
    {
        _interval.Set(interval);
        if (_started)
        {
            Refresh(); // Re-evaluate the current wait with the new interval.
        }
    }

    public void Refresh()
    {
        if (_refresh.CurrentCount == 0)
        {
            _refresh.Release();
        }
    }

    private async Task RunAsync()
    {
        CancellationToken token = _stop.Token;
        while (!token.IsCancellationRequested)
        {
            ProcessStartInfo? start = CommandLocator.Build("codex", "app-server");
            if (start is null)
            {
                Publish(ProviderSnapshot.Of(Id, ProviderState.NotInstalled));
                await DelayOrRefresh(MissingCliRetry, token);
                continue;
            }

            try
            {
                await RunSessionAsync(start, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or JsonException
                                           or System.ComponentModel.Win32Exception or TimeoutException)
            {
                Publish(ProviderSnapshot.Of(Id, ProviderState.Error, ex.GetType().Name));
            }
            finally
            {
                StopProcess();
            }

            await DelayOrRefresh(RestartDelay, token);
        }
    }

    private async Task RunSessionAsync(ProcessStartInfo start, CancellationToken token)
    {
        start.RedirectStandardInput = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.StandardOutputEncoding = System.Text.Encoding.UTF8;

        _process = Process.Start(start) ?? throw new InvalidOperationException("codex app-server did not start");
        _process.ErrorDataReceived += static (_, _) => { }; // Drain stderr; its content is not logged.
        _process.BeginErrorReadLine();
        Task reader = Task.Run(() => ReadLoopAsync(_process, token), token);

        await RequestAsync("initialize", new JsonObject
        {
            ["clientInfo"] = new JsonObject
            {
                ["name"] = "aiusagewidget",
                ["title"] = "AIUsageWidget",
                ["version"] = AppInfo.Version,
            },
        }, token);
        await NotifyAsync("initialized", new JsonObject());

        while (!token.IsCancellationRequested && !_process.HasExited)
        {
            JsonObject response = await RequestAsync("account/rateLimits/read",
                new JsonObject { ["excludeResetCreditDetails"] = true }, token);
            HandleReadResponse(response);
            // Rate-limit notifications keep arriving while waiting, even in manual mode.
            await DelayOrRefresh(_interval.Delay, token);
            if (reader.IsCompleted)
            {
                break;
            }
        }
    }

    private void HandleReadResponse(JsonObject response)
    {
        if (response["error"] is JsonObject error)
        {
            string message = error["message"]?.GetValue<string>() ?? string.Empty;
            bool signedOut = message.Contains("auth", StringComparison.OrdinalIgnoreCase)
                             || message.Contains("login", StringComparison.OrdinalIgnoreCase)
                             || message.Contains("sign", StringComparison.OrdinalIgnoreCase);
            Publish(ProviderSnapshot.Of(Id, signedOut ? ProviderState.NotSignedIn : ProviderState.Error));
            return;
        }

        _windows = CodexRateLimitParser.Parse(response["result"]?["rateLimits"]);
        PublishWindows();
    }

    private void HandleNotification(JsonObject message)
    {
        if (message["method"]?.GetValue<string>() != "account/rateLimits/updated")
        {
            return;
        }
        _windows = CodexRateLimitParser.Merge(_windows, message["params"]?["rateLimits"]);
        PublishWindows();
    }

    private void PublishWindows()
    {
        var (fiveHour, weekly) = _windows;
        Publish(fiveHour is null && weekly is null
            ? ProviderSnapshot.Of(Id, ProviderState.WaitingForData)
            : new ProviderSnapshot(Id, ProviderState.Ok, fiveHour, weekly, DateTimeOffset.Now));
    }

    private async Task ReadLoopAsync(Process process, CancellationToken token)
    {
        StreamReader output = process.StandardOutput;
        while (!token.IsCancellationRequested)
        {
            string? line = await output.ReadLineAsync(token);
            if (line is null)
            {
                break;
            }

            JsonObject? message;
            try
            {
                message = JsonNode.Parse(line) as JsonObject;
            }
            catch (JsonException)
            {
                continue;
            }
            if (message is null)
            {
                continue;
            }

            if (message["id"] is JsonValue idValue && idValue.TryGetValue(out long id) && message["method"] is null)
            {
                if (_pending.TryRemove(id, out var waiter))
                {
                    waiter.TrySetResult(message);
                }
            }
            else if (message["method"] is not null && message["id"] is null)
            {
                HandleNotification(message);
            }
            // Server-initiated requests (approvals and similar) are not used by this client.
        }

        foreach (var waiter in _pending.Values)
        {
            waiter.TrySetException(new IOException("codex app-server closed its output"));
        }
        _pending.Clear();
    }

    private async Task<JsonObject> RequestAsync(string method, JsonObject parameters, CancellationToken token)
    {
        long id = Interlocked.Increment(ref _nextId);
        var waiter = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = waiter;
        await WriteAsync(new JsonObject { ["method"] = method, ["id"] = id, ["params"] = parameters });
        return await waiter.Task.WaitAsync(TimeSpan.FromSeconds(30), token);
    }

    private Task NotifyAsync(string method, JsonObject parameters) =>
        WriteAsync(new JsonObject { ["method"] = method, ["params"] = parameters });

    private async Task WriteAsync(JsonObject message)
    {
        StreamWriter input = _process?.StandardInput ?? throw new InvalidOperationException("not running");
        await input.WriteLineAsync(message.ToJsonString());
        await input.FlushAsync();
    }

    private async Task DelayOrRefresh(TimeSpan delay, CancellationToken token)
    {
        try
        {
            await _refresh.WaitAsync(delay, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

    private void Publish(ProviderSnapshot snapshot)
    {
        Current = snapshot;
        Changed?.Invoke(snapshot);
    }

    private void StopProcess()
    {
        Process? process = Interlocked.Exchange(ref _process, null);
        if (process is null)
        {
            return;
        }
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
        process.Dispose();
    }

    public void Dispose()
    {
        _stop.Cancel();
        StopProcess();
    }
}
