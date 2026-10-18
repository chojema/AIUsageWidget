using AIUsageWidget.Core;
using AIUsageWidget.Localization;
using AIUsageWidget.Tracking;

namespace AIUsageWidget.Tests;

public class PanelPlacementTests
{
    private static readonly ScreenRect Work = new(0, 0, 1920, 1040);

    [Fact]
    public void Sits_below_the_terminal_when_there_is_room()
    {
        var rect = PanelPlacement.Compute(new TerminalWindow(new ScreenRect(100, 100, 800, 500), Work, 1.0));

        Assert.Equal(new ScreenRect(100, 600, 800, 52), rect);
    }

    [Fact]
    public void Overlaps_the_bottom_edge_of_a_maximized_terminal()
    {
        var rect = PanelPlacement.Compute(new TerminalWindow(Work, Work, 1.0));

        Assert.Equal(1040 - 52, rect.Y);
        Assert.Equal(1920, rect.Width);
    }

    [Fact]
    public void Scales_height_with_display_scaling()
    {
        var rect = PanelPlacement.Compute(new TerminalWindow(new ScreenRect(0, 0, 1000, 400), new ScreenRect(0, 0, 3840, 2080), 2.0));

        Assert.Equal(104, rect.Height);
        Assert.Equal(400, rect.Y);
    }

    [Fact]
    public void Stays_inside_the_work_area_and_keeps_a_minimum_width()
    {
        var rect = PanelPlacement.Compute(new TerminalWindow(new ScreenRect(1700, 200, 300, 300), Work, 1.0));

        Assert.True(rect.Right <= Work.Right);
        Assert.Equal(360, rect.Width);
    }
}

public class UsageFormatterTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, "0m")]
    [InlineData(59, "59m")]
    [InlineData(72, "1h 12m")]
    [InlineData(60 * 50, "2d 2h")]
    public void Formats_durations_in_english(int minutes, string expected)
    {
        Assert.Equal(expected, UsageFormatter.Duration(TimeSpan.FromMinutes(minutes), Strings.English));
    }

    [Fact]
    public void Formats_durations_in_korean()
    {
        Assert.Equal("1시간 12분", UsageFormatter.Duration(TimeSpan.FromMinutes(72), Strings.Korean));
    }

    [Fact]
    public void Remaining_is_rounded_and_clamped()
    {
        Assert.Equal("62% left", UsageFormatter.Remaining(new UsageWindow(37.6, null), Strings.English));
        Assert.Equal("0% 남음", UsageFormatter.Remaining(new UsageWindow(130, null), Strings.Korean));
    }

    [Fact]
    public void Reset_includes_relative_time()
    {
        string text = UsageFormatter.Reset(new UsageWindow(10, Now.AddMinutes(90)), Now, Strings.English);

        Assert.StartsWith("resets ", text);
        Assert.EndsWith("(in 1h 30m)", text);
    }

    [Fact]
    public void Language_setting_overrides_system()
    {
        Assert.Same(Strings.Korean, Strings.For("ko"));
        Assert.Same(Strings.English, Strings.For("en"));
    }
}
