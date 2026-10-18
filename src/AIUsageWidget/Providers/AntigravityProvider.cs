using System.Diagnostics;
using System.Text;
using AIUsageWidget.Core;

namespace AIUsageWidget.Providers;

/// <summary>
/// Reads Antigravity limits with the CLI's read-only print-mode command
/// <c>agy -p /usage --output-format json</c>. The arguments go straight to the executable,
/// never through a shell: a shell such as Git Bash may rewrite "/usage" into a file path,
/// which the CLI would then treat as a prompt.
/// </summary>
public sealed class AntigravityProvider : IUsageProvider
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan SignedOutRetry = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MissingCliRetry = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RunTimeout = TimeSpan.FromSeconds(60);

    private readonly CancellationTokenSource _stop = new();
    private readonly SemaphoreSlim _refresh = new(0);
    private readonly object _gate = new();
    private IReadOnlyList<AntigravityGroup> _groups = [];
    private DateTimeOffset? _updatedAt;
    private string? _selectedGroup;
    private bool _halted;

    public AntigravityProvider(string? selectedGroup)
    {
        _selectedGroup = selectedGroup;
    }

    public ProviderId Id => ProviderId.Antigravity;

    public ProviderSnapshot Current { get; private set; } = ProviderSnapshot.Of(ProviderId.Antigravity, ProviderState.Starting);

    public event Action<ProviderSnapshot>? Changed;

    /// <summary>Group names from the last successful read, in the CLI's order.</summary>
    public IReadOnlyList<string> GroupNames
    {
        get
        {
            lock (_gate)
            {
                return _groups.Select(g => g.Name).ToList();
            }
        }
    }

    /// <summary>The group currently shown, after falling back when the selection is unavailable.</summary>
    public string? ShownGroup
    {
        get
        {
            lock (_gate)
            {
                return AntigravityUsageParser.Select(_groups, _selectedGroup)?.Name;
            }
        }
    }

    public void Start() => _ = Task.Run(RunAsync);

    public void Refresh()
    {
        if (_refresh.CurrentCount == 0)
        {
            _refresh.Release();
        }
    }

    public void SelectGroup(string? name)
    {
        lock (_gate)
        {
            _selectedGroup = name;
        }
        if (_groups.Count > 0)
        {
            PublishGroups();
        }
    }

    private async Task RunAsync()
    {
        CancellationToken token = _stop.Token;
        while (!token.IsCancellationRequested && !_halted)
        {
            ProcessStartInfo? start = CommandLocator.Build("agy", "-p", "/usage", "--output-format", "json");
            if (start is null)
            {
                Publish(ProviderSnapshot.Of(Id, ProviderState.NotInstalled));
                await Wait(MissingCliRetry, token);
                continue;
            }

            TimeSpan next = PollInterval;
            try
            {
                (string stdout, string stderr, int exitCode) = await RunAsync(start, token);
                next = Handle(stdout, stderr, exitCode);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or TimeoutException
                                           or System.ComponentModel.Win32Exception)
            {
                Publish(ProviderSnapshot.Of(Id, ProviderState.Error, ex.GetType().Name));
            }

            if (!_halted)
            {
                await Wait(next, token);
            }
        }
    }

    private TimeSpan Handle(string stdout, string stderr, int exitCode)
    {
        var (status, groups) = AntigravityUsageParser.Parse(stdout);
        switch (status)
        {
            case AntigravityParseStatus.Ok:
                lock (_gate)
                {
                    _groups = groups;
                    _updatedAt = DateTimeOffset.Now;
                }
                PublishGroups();
                return PollInterval;

            case AntigravityParseStatus.Unsafe:
                // The CLI did not answer as the read-only command. Stop for this session so that a
                // changed CLI can never keep spending quota in the background.
                _halted = true;
                Publish(ProviderSnapshot.Of(Id, ProviderState.Error, "unexpected-output"));
                return PollInterval;

            default:
                bool signedOut = (stderr + stdout).Contains("not logged in", StringComparison.OrdinalIgnoreCase)
                                 || (stderr + stdout).Contains("not authenticated", StringComparison.OrdinalIgnoreCase);
                Publish(ProviderSnapshot.Of(Id, signedOut ? ProviderState.NotSignedIn : ProviderState.Error,
                    exitCode != 0 ? $"exit {exitCode}" : status.ToString()));
                return signedOut ? SignedOutRetry : PollInterval;
        }
    }

    private void PublishGroups()
    {
        AntigravityGroup? group;
        DateTimeOffset? updated;
        lock (_gate)
        {
            group = AntigravityUsageParser.Select(_groups, _selectedGroup);
            updated = _updatedAt;
        }

        if (group is null || (group.FiveHour is null && group.Weekly is null))
        {
            Publish(new ProviderSnapshot(Id, ProviderState.WaitingForData, Label: group?.Name));
            return;
        }
        Publish(new ProviderSnapshot(Id, ProviderState.Ok, group.FiveHour, group.Weekly, updated, Label: group.Name));
    }

    private static async Task<(string Stdout, string Stderr, int ExitCode)> RunAsync(ProcessStartInfo start, CancellationToken token)
    {
        start.RedirectStandardInput = true; // No terminal, so the CLI never starts an interactive sign-in.
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.StandardOutputEncoding = Encoding.UTF8;
        start.StandardErrorEncoding = Encoding.UTF8;

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("agy did not start");
        process.StandardInput.Close();
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(token);
        Task<string> stderr = process.StandardError.ReadToEndAsync(token);
        try
        {
            await process.WaitForExitAsync(token).WaitAsync(RunTimeout, token);
        }
        catch (TimeoutException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }
        return (await stdout, await stderr, process.ExitCode);
    }

    private async Task Wait(TimeSpan delay, CancellationToken token)
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
        if (snapshot == Current)
        {
            return;
        }
        Current = snapshot;
        Changed?.Invoke(snapshot);
    }

    public void Dispose() => _stop.Cancel();
}
