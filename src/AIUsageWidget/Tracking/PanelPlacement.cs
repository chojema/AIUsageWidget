namespace AIUsageWidget.Tracking;

public static class PanelPlacement
{
    public const double PanelHeightDip = 52;
    public const double MinimumWidthDip = 360;
    public const double WidgetWidthDip = 1060;

    /// <summary>Widget width for the visible sections: room for each usage section and badge.</summary>
    public static double WidgetWidthFor(IReadOnlyList<Core.SectionId> visible, bool seconds)
    {
        double width = 12 + 6 + 6 + 20; // padding, gap and buttons
        for (int i = 0; i < visible.Count; i++)
        {
            Core.SectionId id = visible[i];
            width += id switch
            {
                Core.SectionId.Date => 76,
                Core.SectionId.Weekday => 46,
                Core.SectionId.Time => seconds ? 78 : 60,
                Core.SectionId.Ime => 46,
                _ => 300,
            };
            if (i > 0)
            {
                width += Core.SectionLayout.IsProvider(id) && Core.SectionLayout.IsProvider(visible[i - 1]) ? 16 : 8;
            }
        }
        return Math.Max(width, MinimumWidthDip);
    }

    /// <summary>
    /// Where the standalone widget goes: the saved position when it is still on a screen,
    /// otherwise the bottom-right corner of the primary work area.
    /// </summary>
    public static ScreenRect Widget(int? savedX, int? savedY, IReadOnlyList<ScreenRect> workAreas, ScreenRect primaryWork, double scale,
        double widthDip = WidgetWidthDip)
    {
        int width = (int)Math.Round(widthDip * scale);
        int height = (int)Math.Round(PanelHeightDip * scale);
        if (savedX is { } x && savedY is { } y &&
            workAreas.FirstOrDefault(w => x + width / 2 >= w.X && x + width / 2 < w.Right &&
                                          y + height / 2 >= w.Y && y + height / 2 < w.Bottom) is { Width: > 0 } screen)
        {
            // Keep the whole widget on that screen, for example after the widget became wider.
            return new ScreenRect(
                Math.Clamp(x, screen.X, Math.Max(screen.X, screen.Right - width)),
                Math.Clamp(y, screen.Y, Math.Max(screen.Y, screen.Bottom - height)),
                width, height);
        }

        int margin = (int)Math.Round(16 * scale);
        return new ScreenRect(
            Math.Max(primaryWork.X, primaryWork.Right - width - margin),
            Math.Max(primaryWork.Y, primaryWork.Bottom - height - margin),
            width, height);
    }

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
