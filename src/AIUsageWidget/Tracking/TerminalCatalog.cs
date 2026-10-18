namespace AIUsageWidget.Tracking;

/// <summary>Terminal apps recognized out of the box. More can be added in settings.json.</summary>
public static class TerminalCatalog
{
    /// <summary>Windows process names without ".exe".</summary>
    public static readonly IReadOnlyCollection<string> WindowsProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "WindowsTerminal", "OpenConsole", "conhost", "wezterm-gui", "alacritty", "mintty", "Tabby",
        "Hyper", "Wave", "Warp", "ConEmu", "ConEmu64", "kitty", "ghostty", "rio", "FluentTerminal.App",
    };

    /// <summary>Windows window classes of console hosts.</summary>
    public static readonly IReadOnlyCollection<string> WindowsClasses = new HashSet<string>(StringComparer.Ordinal)
    {
        "CASCADIA_HOSTING_WINDOW_CLASS", "ConsoleWindowClass", "mintty", "VirtualConsoleClass",
        "PseudoConsoleWindow",
    };

    /// <summary>macOS bundle identifiers.</summary>
    public static readonly IReadOnlyCollection<string> MacBundleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "com.apple.Terminal", "com.googlecode.iterm2", "com.github.wez.wezterm", "org.alacritty",
        "io.alacritty", "net.kovidgoyal.kitty", "com.mitchellh.ghostty", "dev.warp.Warp-Stable",
        "org.tabby", "co.zeit.hyper", "dev.commandline.waveterm", "com.raphaelamorim.rio",
    };
}
