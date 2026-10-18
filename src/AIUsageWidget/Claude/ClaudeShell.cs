using System.Diagnostics;

namespace AIUsageWidget.Claude;

/// <summary>
/// Mirrors how Claude Code runs status line commands: on Windows through Git Bash when it is
/// installed, otherwise PowerShell; on macOS and Linux through a POSIX shell.
/// </summary>
public static class ClaudeShell
{
    public enum Kind { Posix, GitBash, PowerShell }

    public static Kind Detect() => OperatingSystem.IsWindows()
        ? FindGitBash() is null ? Kind.PowerShell : Kind.GitBash
        : Kind.Posix;

    /// <summary>The command string that runs <paramref name="executable"/> with one argument in the given shell.</summary>
    public static string QuoteInvocation(string executable, string argument, Kind kind) => kind switch
    {
        Kind.GitBash => $"\"{executable.Replace('\\', '/')}\" {argument}",
        Kind.PowerShell => $"& '{executable.Replace("'", "''")}' {argument}",
        _ => $"'{executable.Replace("'", "'\\''")}' {argument}",
    };

    public static ProcessStartInfo StartInfo(string command)
    {
        ProcessStartInfo info;
        switch (Detect())
        {
            case Kind.GitBash:
                info = new ProcessStartInfo(FindGitBash()!);
                info.ArgumentList.Add("-c");
                info.ArgumentList.Add(command);
                break;
            case Kind.PowerShell:
                info = new ProcessStartInfo("powershell.exe");
                info.ArgumentList.Add("-NoProfile");
                info.ArgumentList.Add("-NonInteractive");
                info.ArgumentList.Add("-Command");
                info.ArgumentList.Add(command);
                break;
            default:
                info = new ProcessStartInfo("/bin/sh");
                info.ArgumentList.Add("-c");
                info.ArgumentList.Add(command);
                break;
        }
        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        return info;
    }

    internal static string? FindGitBash()
    {
        if (Environment.GetEnvironmentVariable("CLAUDE_CODE_GIT_BASH_PATH") is { Length: > 0 } configured &&
            File.Exists(configured))
        {
            return configured;
        }

        var candidates = new List<string>();
        if (Providers.CommandLocator.FindOnWindowsPath("git") is { } git)
        {
            // git.exe lives in ...\Git\cmd or ...\Git\mingw64\bin; bash.exe is in ...\Git\bin.
            for (string? dir = Path.GetDirectoryName(git); dir is not null && candidates.Count < 3;
                 dir = Path.GetDirectoryName(dir))
            {
                candidates.Add(Path.Combine(dir, "bin", "bash.exe"));
            }
        }
        foreach (var folder in new[] { Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.ProgramFilesX86 })
        {
            candidates.Add(Path.Combine(Environment.GetFolderPath(folder), "Git", "bin", "bash.exe"));
        }
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "Git", "bin", "bash.exe"));

        return candidates.FirstOrDefault(File.Exists);
    }
}
