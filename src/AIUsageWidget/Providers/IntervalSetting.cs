namespace AIUsageWidget.Providers;

/// <summary>A refresh interval that can be changed from the UI thread and read from a worker.</summary>
internal sealed class IntervalSetting
{
    private long _ticks;

    public IntervalSetting(TimeSpan? initial) => Set(initial);

    /// <summary>The interval, or <see cref="Timeout.InfiniteTimeSpan"/> for manual refresh.</summary>
    public TimeSpan Delay
    {
        get
        {
            long ticks = Interlocked.Read(ref _ticks);
            return ticks < 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromTicks(ticks);
        }
    }

    public bool IsManual => Interlocked.Read(ref _ticks) < 0;

    public void Set(TimeSpan? interval) => Interlocked.Exchange(ref _ticks, interval?.Ticks ?? -1);
}
