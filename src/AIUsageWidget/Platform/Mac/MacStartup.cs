using System.Security;
using AIUsageWidget.Core;

namespace AIUsageWidget.Platform.Mac;

/// <summary>
/// Start at login through a per-user LaunchAgent. Nothing is written until the user turns it on.
/// The agent is not loaded right away, so turning it on does not start a second copy now.
/// </summary>
public static class MacStartup
{
    public const string Label = "io.github.chojema.aiusagewidget";

    public static string DefaultAgentPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Library", "LaunchAgents", Label + ".plist");

    public static bool IsEnabled() => IsEnabled(DefaultAgentPath, AppInfo.ExecutablePath);

    public static void SetEnabled(bool enabled) => SetEnabled(DefaultAgentPath, AppInfo.ExecutablePath, enabled);

    internal static bool IsEnabled(string agentPath, string executablePath)
    {
        try
        {
            return File.Exists(agentPath) &&
                   File.ReadAllText(agentPath).Contains(SecurityElement.Escape(LaunchTarget(executablePath)), StringComparison.Ordinal);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal static void SetEnabled(string agentPath, string executablePath, bool enabled)
    {
        if (enabled)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(agentPath)!);
            File.WriteAllText(agentPath, BuildPlist(executablePath));
        }
        else if (File.Exists(agentPath))
        {
            File.Delete(agentPath);
        }
    }

    /// <summary>The .app bundle that contains the executable, or null when it does not run from a bundle.</summary>
    internal static string? FindBundle(string executablePath)
    {
        // <bundle>.app/Contents/MacOS/<executable>. macOS paths use '/', so parse them the same on every OS.
        const string marker = ".app/Contents/MacOS/";
        int index = executablePath.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index > 0 && executablePath.IndexOf('/', index + marker.Length) < 0
            ? executablePath[..(index + ".app".Length)]
            : null;
    }

    private static string LaunchTarget(string executablePath) => FindBundle(executablePath) ?? executablePath;

    internal static string BuildPlist(string executablePath)
    {
        // Opening the bundle through LaunchServices starts it like a Finder launch (menu bar app, single instance).
        string? bundle = FindBundle(executablePath);
        string[] arguments = bundle is null ? [executablePath] : ["/usr/bin/open", bundle];
        string items = string.Concat(arguments.Select(a => $"\n    <string>{SecurityElement.Escape(a)}</string>"));
        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
            <plist version="1.0">
            <dict>
              <key>Label</key><string>{Label}</string>
              <key>ProgramArguments</key>
              <array>{items}
              </array>
              <key>RunAtLoad</key><true/>
              <key>LimitLoadToSessionType</key><string>Aqua</string>
            </dict>
            </plist>

            """;
    }
}
