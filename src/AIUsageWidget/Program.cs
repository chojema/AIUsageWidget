using Avalonia;
using AIUsageWidget.Claude;

namespace AIUsageWidget;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Claude Code runs this as its status line command; it must stay fast and never open a window.
        if (args.Length > 0 && args[0] == ClaudeStatusLineBridge.Argument)
        {
            return ClaudeStatusLineBridge.Run();
        }

        Mutex? single = null;
        try
        {
            single = new Mutex(initiallyOwned: true, SingleInstanceName(), out bool created);
            if (!created)
            {
                return 0; // Already running.
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
        {
            // Continue without the single-instance check.
        }

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            single?.Dispose();
        }
        return 0;
    }

    /// <summary>One instance per data folder, so a portable copy with its own data folder can run alongside.</summary>
    private static string SingleInstanceName()
    {
        const string name = "AIUsageWidget.SingleInstance";
        if (Environment.GetEnvironmentVariable("AIUSAGEWIDGET_DATA_DIR") is not { Length: > 0 } custom)
        {
            return name;
        }
        byte[] hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(custom).ToUpperInvariant()));
        return $"{name}.{Convert.ToHexString(hash, 0, 8)}";
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new MacOSPlatformOptions { ShowInDock = false })
            .LogToTrace();
}
