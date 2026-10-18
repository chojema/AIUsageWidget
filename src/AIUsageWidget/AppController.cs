using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using AIUsageWidget.Claude;
using AIUsageWidget.Core;
using AIUsageWidget.Localization;
using AIUsageWidget.Platform.Mac;
using AIUsageWidget.Platform.Windows;
using AIUsageWidget.Providers;
using AIUsageWidget.Tracking;
using AIUsageWidget.Views;

namespace AIUsageWidget;

/// <summary>Connects providers, the foreground tracker, the panel and the tray menu.</summary>
public sealed class AppController : IDisposable
{
    private readonly Application _app;
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly AppSettings _settings = AppSettings.Load();
    private readonly List<IUsageProvider> _providers = [new CodexProvider(), new ClaudeProvider(), new AntigravityProvider()];
    private readonly PanelWindow _panel = new();
    private readonly DispatcherTimer _trackTimer;
    private readonly DispatcherTimer _clockTimer;
    private IForegroundTracker? _tracker;
    private TrayIcon? _tray;
    private Strings _strings;
    private ScreenRect? _placed;
    private bool _nativeStyleApplied;

    public AppController(Application app, IClassicDesktopStyleApplicationLifetime desktop)
    {
        _app = app;
        _desktop = desktop;
        _strings = Strings.For(_settings.Language);
        _trackTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(OperatingSystem.IsMacOS() ? 250 : 150),
            DispatcherPriority.Background, (_, _) => Track());
        _clockTimer = new DispatcherTimer(TimeSpan.FromSeconds(30), DispatcherPriority.Background, (_, _) => Render());
    }

    public void Start()
    {
        _tracker = CreateTracker();
        foreach (IUsageProvider provider in _providers)
        {
            provider.Changed += _ => Dispatcher.UIThread.Post(Render);
            provider.Start();
        }
        BuildTray();
        Render();
        _trackTimer.Start();
        _clockTimer.Start();
    }

    private IForegroundTracker? CreateTracker()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsForegroundTracker(_settings.ExtraTerminals);
        }
        if (OperatingSystem.IsMacOS())
        {
            return new MacForegroundTracker(MacScreens, _settings.ExtraTerminals);
        }
        return null;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    private IReadOnlyList<MacForegroundTracker.ScreenInfo> MacScreens() =>
        _panel.Screens.All.Select(s => new MacForegroundTracker.ScreenInfo(
            ToRect(s.Bounds), ToRect(s.WorkingArea), s.Scaling)).ToList();

    private static ScreenRect ToRect(PixelRect r) => new(r.X, r.Y, r.Width, r.Height);

    private void Track()
    {
        if (_tracker is null)
        {
            return;
        }

        ForegroundInfo info;
        try
        {
            info = _tracker.Poll();
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            _trackTimer.Stop();
            return;
        }

        switch (info.Kind)
        {
            case ForegroundKind.Terminal:
                ShowAt(PanelPlacement.Compute(info.Terminal!), info.Terminal!.Scale);
                break;
            case ForegroundKind.Other:
                HidePanel();
                break;
            case ForegroundKind.Self:
                break; // Keep the current state while the tray menu or the panel itself is active.
        }
    }

    private void ShowAt(ScreenRect rect, double scale)
    {
        bool wasHidden = !_panel.IsVisible;
        if (!wasHidden && _placed == rect)
        {
            return;
        }

        _panel.Position = new PixelPoint(rect.X, rect.Y);
        _panel.Width = rect.Width / scale;
        _panel.Height = rect.Height / scale;
        if (wasHidden)
        {
            Render();
            _panel.Show();
        }

        if (OperatingSystem.IsWindows() && _panel.TryGetPlatformHandle()?.Handle is { } hwnd)
        {
            if (!_nativeStyleApplied)
            {
                WindowsPanelHost.MakeNonActivating(hwnd);
                _nativeStyleApplied = true;
            }
            // Physical pixels avoid rounding drift when the terminal and panel monitors differ in scale.
            WindowsPanelHost.Place(hwnd, rect);
        }
        _placed = rect;
    }

    private void HidePanel()
    {
        if (_panel.IsVisible)
        {
            _panel.Hide();
        }
        _placed = null;
    }

    private void Render() =>
        _panel.Render(_providers.Select(p => p.Current), _strings, DateTimeOffset.Now);

    private void BuildTray()
    {
        var menu = new NativeMenu();

        var language = new NativeMenuItem(_strings.MenuLanguage) { Menu = new NativeMenu() };
        foreach (var (code, label) in new[] { ("system", _strings.MenuSystemLanguage), ("en", "English"), ("ko", "한국어") })
        {
            var item = new NativeMenuItem(label)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                IsChecked = _settings.Language == code,
            };
            item.Click += (_, _) => SetLanguage(code);
            language.Menu.Items.Add(item);
        }
        menu.Items.Add(language);

        bool claudeConnected = ClaudeIntegration.IsInstalled();
        var claude = new NativeMenuItem(claudeConnected ? _strings.MenuDisconnectClaude : _strings.MenuConnectClaude);
        claude.Click += (_, _) => ToggleClaude(!claudeConnected);
        menu.Items.Add(claude);

        if (OperatingSystem.IsWindows())
        {
            var startup = new NativeMenuItem(_strings.MenuStartAtLogin)
            {
                ToggleType = NativeMenuItemToggleType.CheckBox,
                IsChecked = WindowsStartup.IsEnabled(),
            };
            startup.Click += (_, _) =>
            {
                if (OperatingSystem.IsWindows())
                {
                    WindowsStartup.SetEnabled(!WindowsStartup.IsEnabled());
                }
                BuildTray();
            };
            menu.Items.Add(startup);
        }

        var refresh = new NativeMenuItem(_strings.MenuRefresh);
        refresh.Click += (_, _) => _providers.ForEach(p => p.Refresh());
        menu.Items.Add(refresh);

        menu.Items.Add(new NativeMenuItemSeparator());

        var project = new NativeMenuItem($"{_strings.MenuOpenProject} ({AppInfo.Name} {AppInfo.Version})");
        project.Click += (_, _) => OpenUrl(AppInfo.RepositoryUrl);
        menu.Items.Add(project);

        var quit = new NativeMenuItem(_strings.MenuQuit);
        quit.Click += (_, _) => _desktop.Shutdown();
        menu.Items.Add(quit);

        if (_tray is null)
        {
            using Stream icon = AssetLoader.Open(new Uri("avares://AIUsageWidget/Assets/tray.png"));
            _tray = new TrayIcon
            {
                Icon = new WindowIcon(icon),
                ToolTipText = $"{AppInfo.Name} {AppInfo.Version}",
                IsVisible = true,
            };
            TrayIcon.SetIcons(_app, [_tray]);
        }
        _tray.Menu = menu;
    }

    private void SetLanguage(string code)
    {
        _settings.Language = code;
        _settings.Save();
        _strings = Strings.For(code);
        BuildTray();
        Render();
    }

    private void ToggleClaude(bool connect)
    {
        string? error;
        bool ok = connect ? ClaudeIntegration.Install(_settings, out error) : ClaudeIntegration.Uninstall(_settings, out error);
        if (!ok && _tray is not null)
        {
            _tray.ToolTipText = string.Format(_strings.ClaudeConnectFailed, error);
        }
        _providers.First(p => p.Id == ProviderId.ClaudeCode).Refresh();
        BuildTray();
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
    }

    public void Dispose()
    {
        _trackTimer.Stop();
        _clockTimer.Stop();
        foreach (IUsageProvider provider in _providers)
        {
            provider.Dispose();
        }
        if (_tray is not null)
        {
            _tray.IsVisible = false;
            _tray.Dispose();
        }
    }
}
