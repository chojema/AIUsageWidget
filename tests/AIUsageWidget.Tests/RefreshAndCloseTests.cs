using System.Text.Json;
using AIUsageWidget.Core;
using AIUsageWidget.Providers;

namespace AIUsageWidget.Tests;

public class RefreshIntervalTests
{
    [Theory]
    [InlineData(RefreshPreset.OneMinute, 1)]
    [InlineData(RefreshPreset.FiveMinutes, 5)]
    [InlineData(RefreshPreset.ThirtyMinutes, 30)]
    [InlineData(RefreshPreset.OneHour, 60)]
    public void Presets_map_to_their_minutes(RefreshPreset preset, int minutes)
    {
        Assert.Equal(TimeSpan.FromMinutes(minutes), RefreshInterval.For(preset, customMinutes: 99));
    }

    [Fact]
    public void Manual_has_no_automatic_interval()
    {
        Assert.Null(RefreshInterval.For(RefreshPreset.Manual, 10));
    }

    [Theory]
    [InlineData(15, 15)]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(5000, 1440)]
    public void Custom_minutes_are_kept_within_a_day(int input, int expected)
    {
        Assert.Equal(TimeSpan.FromMinutes(expected), RefreshInterval.For(RefreshPreset.Custom, input));
    }

    [Fact]
    public void Interval_setting_reports_manual_as_an_infinite_wait()
    {
        var setting = new IntervalSetting(TimeSpan.FromMinutes(5));
        Assert.Equal(TimeSpan.FromMinutes(5), setting.Delay);
        Assert.False(setting.IsManual);

        setting.Set(null);

        Assert.True(setting.IsManual);
        Assert.Equal(Timeout.InfiniteTimeSpan, setting.Delay);
    }

    [Fact]
    public void New_settings_default_to_five_minutes_and_asking_on_close()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        AppSettings defaults = JsonSerializer.Deserialize<AppSettings>("{}", options)!;
        string saved = JsonSerializer.Serialize(new AppSettings { RefreshPreset = RefreshPreset.Custom, CloseAction = CloseAction.Quit }, options);

        Assert.Equal(RefreshPreset.FiveMinutes, defaults.RefreshPreset);
        Assert.Equal(CloseAction.Ask, defaults.CloseAction);
        Assert.Contains("\"refreshPreset\":\"Custom\"", saved);
        Assert.Contains("\"closeAction\":\"Quit\"", saved);
    }
}
