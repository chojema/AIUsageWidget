using System.Diagnostics;

namespace AIUsageWidget.Providers;

/// <summary>Finds CLI executables and builds a start command for them.</summary>
public static class CommandLocator
{
    /// <summary>
    /// Returns start information for <paramref name="command"/> with <paramref name="arguments"/>,
    /// or null when the command cannot be found.
    /// </summary>
    public static ProcessStartInfo? Build(string command, params string[] arguments)
    {
        ProcessStartInfo info;
        if (OperatingSystem.IsWindows())
        {
            string? path = FindOnWindowsPath(command);
            if (path is null)
            {
                return null;
            }

            if (path.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            {
                info = new ProcessStartInfo("cmd.exe");
                info.ArgumentList.Add("/d");
                info.ArgumentList.Add("/c");
                info.ArgumentList.Add(path);
            }
            else
            {
                info = new ProcessStartInfo(path);
            }

            foreach (string arg in arguments)
            {
                info.ArgumentList.Add(arg);
            }
        }
        else
        {
            // Apps started from Finder or the Dock do not inherit the shell PATH,
            // so run through the user's login shell to find npm or Homebrew installs.
            if (!ExistsInLoginShell(command))
            {
                return null;
            }

            info = new ProcessStartInfo(LoginShell);
            info.ArgumentList.Add("-lc");
            // Quote every argument so the shell passes values such as "/usage" through unchanged.
            info.ArgumentList.Add($"exec {command} {string.Join(' ', arguments.Select(PosixQuote))}");
        }

        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        return info;
    }

    public static bool Exists(string command) =>
        OperatingSystem.IsWindows() ? FindOnWindowsPath(command) is not null : ExistsInLoginShell(command);

    internal static string PosixQuote(string value) => $"'{value.Replace("'", "'\\''")}'";

    private static string LoginShell =>
        Environment.GetEnvironmentVariable("SHELL") is { Length: > 0 } shell && File.Exists(shell) ? shell : "/bin/zsh";

    internal static string? FindOnWindowsPath(string command)
    {
        string[] extensions = [".exe", ".cmd", ".bat"];
        var dirs = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Append(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm"));

        foreach (string dir in dirs)
        {
            foreach (string ext in extensions)
            {
                try
                {
                    string candidate = Path.Combine(dir.Trim('"'), command + ext);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
                catch (ArgumentException)
                {
                    // Skip malformed PATH entries.
                }
            }
        }
        return null;
    }

    private static bool ExistsInLoginShell(string command)
    {
        try
        {
            var info = new ProcessStartInfo(LoginShell)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            info.ArgumentList.Add("-lc");
            info.ArgumentList.Add($"command -v {command}");
            using var process = Process.Start(info);
            if (process is null)
            {
                return false;
            }
            process.StandardOutput.ReadToEnd();
            if (!process.WaitForExit(10_000))
            {
                process.Kill(entireProcessTree: true);
                return false;
            }
            return process.ExitCode == 0;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }
}
