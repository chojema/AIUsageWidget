namespace AIUsageWidget.Tracking;

public static class PanelPlacement
{
    public const double PanelHeightDip = 52;
    public const double MinimumWidthDip = 360;

    /// <summary>
    /// Places the panel directly below the terminal window. When there is no room below
    /// (a maximized or full-screen terminal), it sits over the bottom edge of the terminal instead.
    /// The panel never extends past the work area of the terminal's screen.
    /// </summary>
    public static ScreenRect Compute(TerminalWindow terminal)
    {
        ScreenRect t = terminal.Bounds;
        ScreenRect work = terminal.WorkArea;
        int height = (int)Math.Round(PanelHeightDip * terminal.Scale);

        int left = Math.Max(t.X, work.X);
        int right = Math.Min(t.Right, work.Right);
        int minWidth = (int)Math.Round(MinimumWidthDip * terminal.Scale);
        if (right - left < minWidth)
        {
            right = Math.Min(left + minWidth, work.Right);
            left = Math.Max(work.X, right - minWidth);
        }

        int top = t.Bottom + height <= work.Bottom
            ? t.Bottom
            : Math.Min(t.Bottom, work.Bottom) - height;
        top = Math.Max(top, work.Y);

        return new ScreenRect(left, top, Math.Max(right - left, 1), height);
    }
}
