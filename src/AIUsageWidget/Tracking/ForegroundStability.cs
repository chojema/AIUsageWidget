namespace AIUsageWidget.Tracking;

/// <summary>Requires one uninterrupted foreground interval before attaching to a Windows terminal.</summary>
public sealed class ForegroundStability(TimeProvider? timeProvider = null)
{
    public static readonly TimeSpan DisplayDelay = TimeSpan.FromSeconds(1);
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    private (nint Handle, uint Process, long Generation)? _candidate;
    private long _since;

    public bool IsReady(ForegroundInfo info)
    {
        if (info.Kind != ForegroundKind.Terminal || info.Terminal is not { Handle: not 0 } terminal)
        {
            Reset();
            return false;
        }

        var identity = (terminal.Handle, terminal.ProcessId, terminal.ForegroundGeneration);
        if (_candidate != identity)
        {
            _candidate = identity;
            _since = _time.GetTimestamp();
            return false;
        }

        return _time.GetElapsedTime(_since) >= DisplayDelay;
    }

    public void Reset() => _candidate = null;
}
