using System.Text.Json;
using AIUsageWidget.Core;
using AIUsageWidget.Ime;
using AIUsageWidget.Tracking;

namespace AIUsageWidget.Tests;

public class ImeLabelTests
{
    private const int Korean = 0x0412;
    private const int EnglishUs = 0x0409;
    private const int Japanese = 0x0411;

    [Theory]
    [InlineData(Korean, 0x0001, "한")]
    [InlineData(Korean, 0x0009, "한")] // Hangul plus full-shape bits
    [InlineData(Korean, 0x0000, "EN")]
    [InlineData(EnglishUs, 0x0001, "EN")]
    [InlineData(Japanese, 0x0000, "JA")]
    public void Windows_labels_follow_layout_and_conversion_mode(int languageId, int mode, string expected)
    {
        Assert.Equal(expected, ImeLabel.FromWindows(languageId, mode));
    }

    [Fact]
    public void Windows_korean_layout_without_readable_mode_is_english()
    {
        Assert.Equal("EN", ImeLabel.FromWindows(Korean, null));
    }

    [Theory]
    [InlineData("com.apple.inputmethod.Korean.2SetKorean", "한")]
    [InlineData("com.apple.inputmethod.Korean.Roman", "EN")]
    [InlineData("com.apple.keylayout.ABC", "EN")]
    [InlineData("com.apple.keylayout.US", "EN")]
    [InlineData("com.apple.inputmethod.Kotoeri.RomajiTyping.Japanese", "JA")]
    [InlineData("com.apple.inputmethod.SCIM.ITABC", "ZH")]
    [InlineData(null, "EN")]
    public void Mac_labels_follow_input_source(string? sourceId, string expected)
    {
        Assert.Equal(expected, ImeLabel.FromMac(sourceId));
    }
}

public class WidgetPlacementTests
{
    private static readonly ScreenRect Primary = new(0, 0, 1920, 1040);
    private static readonly ScreenRect Second = new(1920, 0, 2560, 1400);

    [Fact]
    public void Defaults_to_the_bottom_right_of_the_primary_screen()
    {
        ScreenRect rect = PanelPlacement.Widget(null, null, [Primary], Primary, 1.0);

        Assert.Equal(1920 - 1060 - 16, rect.X);
        Assert.Equal(1040 - 52 - 16, rect.Y);
        Assert.Equal(1060, rect.Width);
    }

    [Fact]
    public void Keeps_a_saved_position_that_is_still_on_a_screen()
    {
        ScreenRect rect = PanelPlacement.Widget(2100, 300, [Primary, Second], Primary, 1.0);

        Assert.Equal((2100, 300), (rect.X, rect.Y));
    }

    [Fact]
    public void Pulls_a_saved_position_back_inside_the_screen()
    {
        ScreenRect rect = PanelPlacement.Widget(1084, 1012, [Primary], Primary, 1.0);

        Assert.Equal(Primary.Right - 1060, rect.X);
        Assert.True(rect.Bottom <= Primary.Bottom);
    }

    [Fact]
    public void Moves_back_when_the_saved_screen_is_gone()
    {
        ScreenRect rect = PanelPlacement.Widget(2100, 300, [Primary], Primary, 1.0);

        Assert.True(rect.Right <= Primary.Right);
    }
}

public class AppSettingsFormatTests
{
    [Fact]
    public void Display_mode_is_saved_as_text_and_defaults_are_safe()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        string json = JsonSerializer.Serialize(new AppSettings { DisplayMode = DisplayMode.Widget }, options);
        AppSettings defaults = JsonSerializer.Deserialize<AppSettings>("{}", options)!;

        Assert.Contains("\"displayMode\":\"Widget\"", json);
        Assert.Equal(DisplayMode.Terminal, defaults.DisplayMode);
        Assert.True(defaults.ShowIme);
        Assert.True(defaults.WidgetTopmost);
    }
}
