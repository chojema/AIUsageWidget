using AIUsageWidget.Claude;
using AIUsageWidget.Core;

namespace AIUsageWidget.Providers;

/// <summary>
/// Shows the limits that Claude Code passed to its status line, as saved by
/// <see cref="ClaudeStatusLineBridge"/>. Claude Code only reports them while a session is running.
/// </summary>
public sealed class ClaudeProvider : IUsageProvider
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private readonly Timer _timer;
    private bool _claudePresent;
    private int _busy;

    public ClaudeProvider()
    {
        _timer = new Timer(_ => Poll(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public ProviderId Id => ProviderId.ClaudeCode;

    public ProviderSnapshot Current { get; private set; } = ProviderSnapshot.Of(ProviderId.ClaudeCode, ProviderState.Starting);

    public event Action<ProviderSnapshot>? Changed;

    public void Start()
    {
        _ = Task.Run(() =>
        {
            _claudePresent = DetectClaude();
            ClaudeIntegration.RepointIfMoved();
            _timer.Change(TimeSpan.Zero, PollInterval);
        });
    }

    public void Refresh()
    {
        _ = Task.Run(() =>
        {
            _claudePresent = DetectClaude();
            Poll();
        });
    }

    private static bool DetectClaude() =>
        Directory.Exists(AppPaths.ClaudeConfigDirectory) || CommandLocator.Exists("claude");

    private void Poll()
    {
        if (Interlocked.Exchange(ref _busy, 1) == 1)
        {
            return;
        }
        try
        {
            Publish(Read(DateTimeOffset.Now));
        }
        finally
        {
            Volatile.Write(ref _busy, 0);
        }
    }

    private ProviderSnapshot Read(DateTimeOffset now)
    {
        if (!ClaudeIntegration.IsInstalled())
        {
            return ProviderSnapshot.Of(Id, _claudePresent ? ProviderState.NotConnected : ProviderState.NotInstalled);
        }

        ClaudeUsageRecord? record = null;
        try
        {
            if (File.Exists(AppPaths.ClaudeUsageCache))
            {
                record = ClaudeUsageRecord.FromJson(File.ReadAllText(AppPaths.ClaudeUsageCache));
            }
        }
        catch (IOException)
        {
            return Current; // The bridge is writing; try again on the next tick.
        }

        return ToSnapshot(record, now);
    }

    internal static ProviderSnapshot ToSnapshot(ClaudeUsageRecord? record, DateTimeOffset now)
    {
        if (record is null)
        {
            return ProviderSnapshot.Of(ProviderId.ClaudeCode, ProviderState.WaitingForData);
        }

        // A window whose reset time has passed no longer describes the current limit.
        UsageWindow? fiveHour = record.FiveHour is { } f && !f.HasExpired(now) ? f : null;
        UsageWindow? weekly = record.Weekly is { } w && !w.HasExpired(now) ? w : null;
        return fiveHour is null && weekly is null
            ? new ProviderSnapshot(ProviderId.ClaudeCode, ProviderState.WaitingForData, UpdatedAt: record.CapturedAt)
            : new ProviderSnapshot(ProviderId.ClaudeCode, ProviderState.Ok, fiveHour, weekly, record.CapturedAt);
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

    public void Dispose() => _timer.Dispose();
}
