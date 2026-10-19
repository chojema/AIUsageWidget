namespace AIUsageWidget.Tracking;

/// <summary>A rectangle in physical screen pixels.</summary>
public readonly record struct ScreenRect(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;
}

/// <summary>The frontmost terminal window and the screen it is on.</summary>
public sealed record TerminalWindow(ScreenRect Bounds, ScreenRect WorkArea, double Scale)
{
    // Windows identity and activation generation distinguish windows even at identical bounds.
    public nint Handle { get; init; }
    public uint ProcessId { get; init; }
    public long ForegroundGeneration { get; init; }
}

public enum ForegroundKind
{
    /// <summary>A supported terminal window is in front.</summary>
    Terminal,
    /// <summary>Another app is in front, or the terminal window is minimized.</summary>
    Other,
    /// <summary>This app itself is in front (for example while its tray menu is open).</summary>
    Self,
}

public sealed record ForegroundInfo(ForegroundKind Kind, TerminalWindow? Terminal = null)
{
    public static ForegroundInfo Other { get; } = new(ForegroundKind.Other);
    public static ForegroundInfo Self { get; } = new(ForegroundKind.Self);
}

public interface IForegroundTracker
{
    ForegroundInfo Poll();
}
