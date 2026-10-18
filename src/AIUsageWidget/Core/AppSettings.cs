using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AIUsageWidget.Core;

public sealed class AppSettings
{
    /// <summary>"system", "en" or "ko".</summary>
    public string Language { get; set; } = "system";

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
