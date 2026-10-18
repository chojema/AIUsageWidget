using System.Diagnostics;
using System.Text;
using AIUsageWidget.Core;

namespace AIUsageWidget.Claude;

/// <summary>
/// Runs when Claude Code invokes <c>AIUsageWidget claude-statusline</c> as its status line command.
/// Saves the rate-limit windows, then hands the same input to the user's previous status line
/// command (if any) and prints its output, so the visible status line stays as it was.
/// </summary>
public static class ClaudeStatusLineBridge
{
    public const string Argument = "claude-statusline";

    private static readonly TimeSpan ForwardTimeout = TimeSpan.FromSeconds(10);

    public static int Run()
    {
        string input;
        using (var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8))
        {
            input = reader.ReadToEnd();
        }

        try
        {
            if (ClaudeUsageRecord.FromStatusLineInput(input, DateTimeOffset.Now) is { } record)
            {
                AppPaths.WriteAtomically(AppPaths.ClaudeUsageCache, record.ToJson());
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Never break the user's status line because the cache could not be written.
        }

        string? previous = AppSettings.Load().ClaudePreviousStatusLine?["command"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(previous))
        {
            return 0;
        }

        try
        {
            ProcessStartInfo info = ClaudeShell.StartInfo(previous);
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.StandardInputEncoding = new UTF8Encoding(false);
            info.StandardOutputEncoding = Encoding.UTF8;

            using Process process = Process.Start(info)!;
            Task<string> output = process.StandardOutput.ReadToEndAsync();
            process.StandardInput.Write(input);
            process.StandardInput.Close();

            if (!process.WaitForExit(ForwardTimeout))
            {
                process.Kill(entireProcessTree: true);
                return 0;
            }

            using Stream stdout = Console.OpenStandardOutput();
            byte[] bytes = new UTF8Encoding(false).GetBytes(output.Result);
            stdout.Write(bytes);
            return process.ExitCode;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or InvalidOperationException)
        {
            return 0;
        }
    }
}
