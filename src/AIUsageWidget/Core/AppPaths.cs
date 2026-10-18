namespace AIUsageWidget.Core;

/// <summary>Per-user locations for the app's own settings and caches.</summary>
public static class AppPaths
{
    public static string DataDirectory
    {
        get
        {
            // Allows a portable setup or an isolated test run.
            if (Environment.GetEnvironmentVariable("AIUSAGEWIDGET_DATA_DIR") is { Length: > 0 } custom)
            {
                Directory.CreateDirectory(custom);
                return custom;
            }

            string root = OperatingSystem.IsMacOS()
                ? Path.Combine(Home, "Library", "Application Support")
                : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(root, "AIUsageWidget");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");

    /// <summary>Latest Claude Code limits written by the status line bridge.</summary>
    public static string ClaudeUsageCache => Path.Combine(DataDirectory, "claude-usage.json");

    public static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static string ClaudeConfigDirectory =>
        Environment.GetEnvironmentVariable("CLAUDE_CONFIG_DIR") is { Length: > 0 } custom
            ? custom
            : Path.Combine(Home, ".claude");

    public static string ClaudeSettingsFile => Path.Combine(ClaudeConfigDirectory, "settings.json");

    /// <summary>Writes a file so that readers never observe a partial result.</summary>
    public static void WriteAtomically(string path, string contents)
    {
        string temp = $"{path}.{Environment.ProcessId}.tmp";
        File.WriteAllText(temp, contents);
        File.Move(temp, path, overwrite: true);
    }
}
