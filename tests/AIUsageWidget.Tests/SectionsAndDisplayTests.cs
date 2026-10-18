using System.Text.Json;
using AIUsageWidget.Core;
using AIUsageWidget.Localization;
using AIUsageWidget.Tracking;

namespace AIUsageWidget.Tests;

public class SectionLayoutTests
{
    [Fact]
    public void Fresh_settings_show_every_section_in_the_default_order()
    {
        List<SectionSetting> sections = SectionLayout.Normalize(null);

        Assert.Equal(SectionLayout.DefaultOrder, sections.Select(s => s.Id));
        Assert.All(sections, s => Assert.True(s.Visible));
    }

    [Fact]
    public void Date_weekday_and_time_sit_left_of_the_input_state_by_default()
    {
        var order = SectionLayout.DefaultOrder.ToList();

        Assert.True(order.IndexOf(SectionId.Date) < order.IndexOf(SectionId.Ime));
        Assert.True(order.IndexOf(SectionId.Weekday) < order.IndexOf(SectionId.Ime));
        Assert.True(order.IndexOf(SectionId.Time) < order.IndexOf(SectionId.Ime));
        Assert.Equal(SectionId.Ime, order[^1]);
    }

    [Fact]
    public void Older_settings_keep_the_input_state_choice()
    {
        Assert.False(SectionLayout.Normalize(null, legacyShowIme: false).Single(s => s.Id == SectionId.Ime).Visible);
    }

    [Fact]
    public void Saved_order_is_kept_and_missing_sections_are_added()
    {
        var saved = new List<SectionSetting>
        {
            new() { Id = SectionId.Ime, Visible = true },
            new() { Id = SectionId.Codex, Visible = false },
            new() { Id = SectionId.Codex, Visible = true }, // duplicate is ignored
        };

        List<SectionSetting> sections = SectionLayout.Normalize(saved);

        Assert.Equal(7, sections.Count);
        Assert.Equal(SectionId.Ime, sections[0].Id);
        Assert.Equal(SectionId.Codex, sections[1].Id);
        Assert.False(sections[1].Visible);
        Assert.Equal(SectionId.ClaudeCode, sections[2].Id); // follows Codex as in the default order
        Assert.Equal(7, sections.Select(s => s.Id).Distinct().Count());
    }

    [Fact]
    public void Sections_added_in_a_new_version_go_next_to_their_default_neighbors()
    {
        var older = new List<SectionSetting>
        {
            new() { Id = SectionId.Antigravity }, new() { Id = SectionId.Codex },
            new() { Id = SectionId.ClaudeCode }, new() { Id = SectionId.Ime },
        };

        var ids = SectionLayout.Normalize(older).Select(s => s.Id).ToList();

        Assert.Equal([SectionId.Antigravity, SectionId.Date, SectionId.Weekday, SectionId.Time,
                      SectionId.Codex, SectionId.ClaudeCode, SectionId.Ime], ids);
    }

    [Fact]
    public void Moving_swaps_neighbors_and_stops_at_the_ends()
    {
        List<SectionSetting> sections = SectionLayout.Normalize(null);

        Assert.True(SectionLayout.Move(sections, SectionId.Ime, -1));
        Assert.Equal(SectionId.Ime, sections[^2].Id);
        Assert.False(SectionLayout.Move(sections, SectionId.Codex, -1));
        Assert.False(SectionLayout.Move(sections, SectionId.Time, +1));
    }

    [Fact]
    public void Only_visible_sections_are_laid_out()
    {
        List<SectionSetting> sections = SectionLayout.Normalize(null);
        sections.First(s => s.Id == SectionId.Antigravity).Visible = false;

        Assert.DoesNotContain(SectionId.Antigravity, SectionLayout.Visible(sections));
        Assert.Equal(6, SectionLayout.Visible(sections).Count);
    }

    [Fact]
    public void Sections_are_saved_with_readable_names()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        string json = JsonSerializer.Serialize(new AppSettings { Sections = SectionLayout.Normalize(null), UsageDisplay = UsageDisplay.Used }, options);

        Assert.Contains("\"id\":\"ClaudeCode\"", json);
        Assert.Contains("\"usageDisplay\":\"Used\"", json);
        AppSettings back = JsonSerializer.Deserialize<AppSettings>(json, options)!;
        Assert.Equal(SectionLayout.DefaultOrder, back.Sections!.Select(s => s.Id));
    }
}

public class UsageDisplayAndClockTests
{
    [Fact]
    public void Usage_can_be_shown_as_left_or_used()
    {
        var window = new UsageWindow(37.6, null);

        Assert.Equal("62% left", UsageFormatter.Percent(window, Strings.English, UsageDisplay.Remaining));
        Assert.Equal("38% used", UsageFormatter.Percent(window, Strings.English, UsageDisplay.Used));
        Assert.Equal("38% 사용", UsageFormatter.Percent(window, Strings.Korean, UsageDisplay.Used));
        Assert.Equal("100% used", UsageFormatter.Percent(new UsageWindow(130, null), Strings.English, UsageDisplay.Used));
    }

    [Fact]
    public void Clock_text_follows_language_and_seconds_option()
    {
        var local = new DateTimeOffset(2026, 10, 18, 9, 5, 7, TimeZoneInfo.Local.GetUtcOffset(new DateTime(2026, 10, 18, 9, 5, 7)));

        Assert.Equal("Oct 18", UsageFormatter.Date(local, Strings.English));
        Assert.Equal("10월 18일", UsageFormatter.Date(local, Strings.Korean));
        Assert.Equal("Sun", UsageFormatter.Weekday(local, Strings.English));
        Assert.Equal("일", UsageFormatter.Weekday(local, Strings.Korean));
        Assert.Equal("09:05", UsageFormatter.Time(local, seconds: false));
        Assert.Equal("09:05:07", UsageFormatter.Time(local, seconds: true));
    }

    [Fact]
    public void Widget_width_follows_the_visible_sections()
    {
        double all = PanelPlacement.WidgetWidthFor(SectionLayout.DefaultOrder, seconds: false);
        double withSeconds = PanelPlacement.WidgetWidthFor(SectionLayout.DefaultOrder, seconds: true);
        double clockOnly = PanelPlacement.WidgetWidthFor([SectionId.Time], seconds: false);

        Assert.True(withSeconds > all);
        Assert.Equal(PanelPlacement.MinimumWidthDip, clockOnly);
        Assert.True(PanelPlacement.WidgetWidthFor([SectionId.Codex, SectionId.ClaudeCode], false) < all);
    }
}
