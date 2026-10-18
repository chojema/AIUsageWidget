using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AIUsageWidget.Core;

[JsonConverter(typeof(JsonStringEnumConverter<DisplayMode>))]
public enum DisplayMode
{
    /// <summary>Shown below the frontmost terminal window and hidden for other apps.</summary>
    Terminal,
    /// <summary>Always shown where the user placed it.</summary>
    Widget,
}

public sealed class AppSettings
{
    /// <summary>"system", "en" or "ko".</summary>
    public string Language { get; set; } = "system";

    /// <summary>How the panel is shown: attached below the terminal, or as a free-standing widget.</summary>
    public DisplayMode DisplayMode { get; set; } = DisplayMode.Terminal;

    /// <summary>Widget position in physical screen pixels; null until the widget is first placed.</summary>
    public int? WidgetX { get; set; }

    public int? WidgetY { get; set; }

    /// <summary>Keeps the widget above other windows.</summary>
    public bool WidgetTopmost { get; set; } = true;

    /// <summary>Shows the keyboard input state (for example EN or 한) at the right end of the panel.</summary>
    public bool ShowIme { get; set; } = true;

    /// <summary>The Antigravity model group shown in the panel; null uses the default group.</summary>
    public string? AntigravityGroup { get; set; }

    /// <summary>The status line configuration that existed before the Claude Code bridge was installed.</summary>
    public JsonObject? ClaudePreviousStatusLine { get; set; }

    /// <summary>True when the bridge was installed by this app.</summary>
    public bool ClaudeBridgeInstalled { get; set; }

    /// <summary>Extra terminal process names (Windows) or bundle identifiers (macOS).</summary>
    public List<string> ExtraTerminals { get; set; } = [];

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(AppPaths.SettingsFile))
            {
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(AppPaths.SettingsFile), Options)
                       ?? new AppSettings();
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // A damaged settings file falls back to defaults instead of blocking startup.
        }
        return new AppSettings();
    }

    public void Save() => AppPaths.WriteAtomically(AppPaths.SettingsFile, JsonSerializer.Serialize(this, Options));
}
