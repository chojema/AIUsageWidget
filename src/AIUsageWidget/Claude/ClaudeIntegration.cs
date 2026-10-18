using System.Text.Json;
using System.Text.Json.Nodes;
using AIUsageWidget.Core;

namespace AIUsageWidget.Claude;

/// <summary>
/// Adds or removes the status line bridge in Claude Code's user settings.
/// Only the <c>statusLine</c> key is changed; the previous value is kept and restored on removal.
/// </summary>
public static class ClaudeIntegration
{
    private static readonly JsonDocumentOptions ReadOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    public static string BridgeCommand(string executablePath) =>
        ClaudeShell.QuoteInvocation(executablePath, ClaudeStatusLineBridge.Argument, ClaudeShell.Detect());

    public static bool IsBridgeCommand(string? command) =>
        command is not null &&
        command.Contains(ClaudeStatusLineBridge.Argument, StringComparison.Ordinal) &&
        command.Contains(AppInfo.Name, StringComparison.OrdinalIgnoreCase);

    public static bool IsInstalled()
    {
        try
        {
            return ReadSettings() is { } settings && IsBridgeCommand(CommandOf(settings));
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Installs the bridge. Returns false with a reason when Claude Code settings cannot be changed safely.</summary>
    public static bool Install(AppSettings appSettings, out string? error)
    {
        error = null;
        try
        {
            JsonObject settings = ReadSettings() ?? new JsonObject();
            var (updated, previous) = ApplyInstall(settings, BridgeCommand(AppInfo.ExecutablePath));
            if (!appSettings.ClaudeBridgeInstalled)
            {
                appSettings.ClaudePreviousStatusLine = previous;
            }
            BackupOnce();
            WriteSettings(updated);
            appSettings.ClaudeBridgeInstalled = true;
            appSettings.Save();
            return true;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool Uninstall(AppSettings appSettings, out string? error)
    {
        error = null;
        try
        {
            if (ReadSettings() is { } settings)
            {
                WriteSettings(ApplyUninstall(settings, appSettings.ClaudePreviousStatusLine));
            }
            appSettings.ClaudeBridgeInstalled = false;
            appSettings.ClaudePreviousStatusLine = null;
            appSettings.Save();
            return true;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Points an installed bridge at the current executable after the app was moved.</summary>
    public static void RepointIfMoved()
    {
        try
        {
            if (ReadSettings() is not { } settings || !IsBridgeCommand(CommandOf(settings)))
            {
                return;
            }
            string expected = BridgeCommand(AppInfo.ExecutablePath);
            if (CommandOf(settings) != expected && settings["statusLine"] is JsonObject line)
            {
                line["command"] = expected;
                WriteSettings(settings);
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
        }
    }

    internal static (JsonObject Updated, JsonObject? Previous) ApplyInstall(JsonObject settings, string bridgeCommand)
    {
        JsonObject? existing = settings["statusLine"] as JsonObject;
        JsonObject? previous = existing is not null && !IsBridgeCommand(existing["command"]?.GetValue<string>())
            ? (JsonObject)existing.DeepClone()
            : null;

        // Keep padding, refreshInterval and other display options of the previous status line.
        JsonObject line = existing is not null ? (JsonObject)existing.DeepClone() : new JsonObject();
        line["type"] = "command";
        line["command"] = bridgeCommand;
        settings["statusLine"] = line;
        return (settings, previous);
    }

    internal static JsonObject ApplyUninstall(JsonObject settings, JsonObject? previous)
    {
        if (!IsBridgeCommand(CommandOf(settings)))
        {
            return settings; // Someone else changed it since; leave their value alone.
        }
        if (previous is not null)
        {
            settings["statusLine"] = previous.DeepClone();
        }
        else
        {
            settings.Remove("statusLine");
        }
        return settings;
    }

    private static string? CommandOf(JsonObject settings) =>
        (settings["statusLine"] as JsonObject)?["command"]?.GetValue<string>();

    private static JsonObject? ReadSettings()
    {
        string path = AppPaths.ClaudeSettingsFile;
        if (!File.Exists(path))
        {
            return null;
        }
        return JsonNode.Parse(File.ReadAllText(path), documentOptions: ReadOptions) as JsonObject
               ?? throw new JsonException("Claude Code settings.json is not a JSON object.");
    }

    private static void WriteSettings(JsonObject settings)
    {
        Directory.CreateDirectory(AppPaths.ClaudeConfigDirectory);
        AppPaths.WriteAtomically(AppPaths.ClaudeSettingsFile, settings.ToJsonString(WriteOptions) + Environment.NewLine);
    }

    private static void BackupOnce()
    {
        string path = AppPaths.ClaudeSettingsFile;
        string backup = path + ".aiusagewidget-backup";
        if (File.Exists(path) && !File.Exists(backup))
        {
            File.Copy(path, backup);
        }
    }
}
