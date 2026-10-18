using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using AIUsageWidget.Claude;
using AIUsageWidget.Core;
using AIUsageWidget.Ime;
using AIUsageWidget.Localization;
using AIUsageWidget.Platform.Mac;
using AIUsageWidget.Platform.Windows;
using AIUsageWidget.Providers;
using AIUsageWidget.Tracking;
using AIUsageWidget.Views;

namespace AIUsageWidget;

/// <summary>Connects providers, the foreground tracker, the panel, the settings window and the tray menu.</summary>
public sealed class AppController : IDisposable, ISettingsHost
{
    private readonly Application _app;
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly AppSettings _settings = AppSettings.Load();
    private readonly List<SectionSetting> _sections;
    private readonly AntigravityProvider _antigravity;
    private readonly List<IUsageProvider> _providers;
    private readonly PanelWindow _panel = new();
    private readonly DispatcherTimer _trackTimer;
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _saveWidgetTimer;
    private readonly DispatcherTimer _secondTimer;
    private IForegroundTracker? _tracker;
    private IImeReader? _imeReader;
    private TrayIcon? _tray;
    private SettingsWindow? _settingsWindow;
    private Strings _strings;
    private ScreenRect? _placed;
    private string? _imeLabel;
    private bool _nativeStyleApplied;
    private bool _placingWidget;
    private string _knownGroups = string.Empty;
    private bool _hiddenByUser;

    public AppController(Application app, IClassicDesktopStyleApplicationLifetime desktop)
    {
        _app = app;
        _desktop = desktop;
        _strings = Strings.For(_settings.Language);
        _sections = SectionLayout.Normalize(_settings.Sections, _settings.ShowIme);
        _antigravity = new AntigravityProvider(_settings.AntigravityGroup);
        _providers = [new CodexProvider(), new ClaudeProvider(), _antigravity];
        _trackTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(OperatingSystem.IsMacOS() ? 250 : 150),
            DispatcherPriority.Background, (_, _) => Tick());
        _clockTimer = new DispatcherTimer(TimeSpan.FromSeconds(30), DispatcherPriority.Background, (_, _) => Render());
        _saveWidgetTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(600), DispatcherPriority.Background, (_, _) => SaveWidgetPosition());
        _secondTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, (_, _) => RenderClock());
        _panel.PositionChanged += (_, _) => OnPanelMoved();
        _panel.RefreshClicked += RefreshAll;
        _panel.CloseClicked += OnCloseClicked;
        _panel.HidePanelChosen += HideByUser;
        _panel.QuitChosen += () => _desktop.Shutdown();
    }

    public void Start()
    {
        (_tracker, _imeReader) = CreatePlatformServices();
        ApplyRefreshInterval();
        foreach (IUsageProvider provider in _providers)
        {
            provider.Changed += _ => Dispatcher.UIThread.Post(OnProviderChanged);
            provider.Start();
        }
        BuildTray();
        _panel.ApplyLayout(VisibleSections);
        Render();
        ApplyDisplayMode();
        _trackTimer.Start();
        _clockTimer.Start();
        _secondTimer.Start();
    }

    private (IForegroundTracker?, IImeReader?) CreatePlatformServices()
    {
        if (OperatingSystem.IsWindows())
        {
            return (new WindowsForegroundTracker(_settings.ExtraTerminals), new WindowsImeReader());
        }
        if (OperatingSystem.IsMacOS())
        {
            return (new MacForegroundTracker(MacScreens, _settings.ExtraTerminals), new MacImeReader());
        }
        return (null, null);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    private IReadOnlyList<MacForegroundTracker.ScreenInfo> MacScreens() =>
        _panel.Screens.All.Select(s => new MacForegroundTracker.ScreenInfo(
            ToRect(s.Bounds), ToRect(s.WorkingArea), s.Scaling)).ToList();

    private static ScreenRect ToRect(PixelRect r) => new(r.X, r.Y, r.Width, r.Height);

    // ----- Polling -----

    private void Tick()
    {
        ForegroundInfo info;
        try
        {
            info = _tracker?.Poll() ?? ForegroundInfo.Other;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            _tracker = null;
            info = ForegroundInfo.Other;
        }

        if (info.Kind != ForegroundKind.Self)
        {
            UpdateIme();
        }

        if (_hiddenByUser || _settings.DisplayMode == DisplayMode.Widget)
        {
            return; // Hidden on request, or a widget that stays where the user put it.
        }

        switch (info.Kind)
        {
            case ForegroundKind.Terminal:
                ShowAttached(PanelPlacement.Compute(info.Terminal!), info.Terminal!.Scale);
                break;
            case ForegroundKind.Other:
                HidePanel();
                break;
            case ForegroundKind.Self:
                break; // Keep the current state while the tray menu or settings window is active.
        }
    }

    private void UpdateIme()
    {
        if (!VisibleSections.Contains(SectionId.Ime) || _imeReader is null)
        {
            return;
        }
        string? label;
        try
        {
            label = _imeReader.Read();
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            _imeReader = null;
            label = null;
        }
        if (label != _imeLabel)
        {
            _imeLabel = label;
            _panel.RenderIme(label, _strings);
        }
    }

    // ----- Display modes -----

    private void ApplyDisplayMode()
    {
        _placed = null;
        if (_hiddenByUser)
        {
            HidePanel();
            return;
        }
        _panel.Draggable = _settings.DisplayMode == DisplayMode.Widget;
        if (_settings.DisplayMode == DisplayMode.Widget)
        {
            ShowWidget();
        }
        else
        {
            _panel.Topmost = true;
            HidePanel(); // The next tick shows it again below a terminal in front.
        }
    }

    private void ShowWidget()
    {
        Screens screens = _panel.Screens;
        Screen? primary = screens.Primary ?? screens.All.FirstOrDefault();
        if (primary is null)
        {
            return;
        }
        Screen target = _settings.WidgetX is { } x && _settings.WidgetY is { } y
            ? screens.ScreenFromPoint(new PixelPoint(x, y)) ?? primary
            : primary;

        ScreenRect rect = PanelPlacement.Widget(_settings.WidgetX, _settings.WidgetY,
            screens.All.Select(s => ToRect(s.WorkingArea)).ToList(), ToRect(primary.WorkingArea), target.Scaling,
            WidgetWidthDip);

        _placingWidget = true;
        _panel.Width = WidgetWidthDip;
        _panel.Height = PanelPlacement.PanelHeightDip;
        _panel.Position = new PixelPoint(rect.X, rect.Y);
        _panel.Topmost = _settings.WidgetTopmost;
        if (!_panel.IsVisible)
        {
            Render();
            _panel.Show();
        }
        ApplyNativeStyle();
        _panel.Position = new PixelPoint(rect.X, rect.Y);
        _placingWidget = false;
    }

    private void ShowAttached(ScreenRect rect, double scale)
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

        if (ApplyNativeStyle() is { } hwnd && OperatingSystem.IsWindows())
        {
            // Physical pixels avoid rounding drift when the terminal and panel monitors differ in scale.
            WindowsPanelHost.Place(hwnd, rect);
        }
        _placed = rect;
    }

    /// <summary>Makes the panel non-activating on Windows. Returns its window handle there.</summary>
    private nint? ApplyNativeStyle()
    {
        if (!OperatingSystem.IsWindows() || _panel.TryGetPlatformHandle()?.Handle is not { } hwnd)
        {
            return null;
        }
        if (!_nativeStyleApplied)
        {
            WindowsPanelHost.MakeNonActivating(hwnd);
            _nativeStyleApplied = true;
        }
        return hwnd;
    }

    private void HidePanel()
    {
        if (_panel.IsVisible)
        {
            _panel.Hide();
        }
        _placed = null;
    }

    private void OnPanelMoved()
    {
        if (_settings.DisplayMode == DisplayMode.Widget && !_placingWidget && _panel.IsVisible)
        {
            _saveWidgetTimer.Stop();
            _saveWidgetTimer.Start();
        }
    }

    private void SaveWidgetPosition()
    {
        _saveWidgetTimer.Stop();
        _settings.WidgetX = _panel.Position.X;
        _settings.WidgetY = _panel.Position.Y;
        _settings.Save();
    }

    private void OnProviderChanged()
    {
        Render();
        // The settings window lists Antigravity groups; rebuild it when the list first arrives or changes.
        string groups = string.Join('\n', _antigravity.GroupNames);
        if (groups != _knownGroups)
        {
            _knownGroups = groups;
            _settingsWindow?.Build();
        }
    }

    private IReadOnlyList<SectionId> VisibleSections => SectionLayout.Visible(_sections);

    private double WidgetWidthDip => PanelPlacement.WidgetWidthFor(VisibleSections, _settings.ShowSeconds);

    private void Render()
    {
        _panel.Render(_providers.Select(p => p.Current), _strings, DateTimeOffset.Now, _settings.UsageDisplay);
        _panel.RenderIme(_imeLabel, _strings);
        RenderClock();
    }

    private void RenderClock() => _panel.RenderClock(DateTimeOffset.Now, _strings, _settings.ShowSeconds);

    /// <summary>Saves the section list and lays the panel out again.</summary>
    private void ApplySections()
    {
        _settings.Sections = _sections.Select(s => new SectionSetting { Id = s.Id, Visible = s.Visible }).ToList();
        _settings.Save();
        _panel.ApplyLayout(VisibleSections);
        Render();
        if (_settings.DisplayMode == DisplayMode.Widget && !_hiddenByUser)
        {
            ShowWidget(); // The widget width follows the visible sections.
        }
    }

    private void RefreshAll() => _providers.ForEach(p => p.Refresh());

    private void ApplyRefreshInterval()
    {
        TimeSpan? interval = RefreshInterval.For(_settings.RefreshPreset, _settings.CustomRefreshMinutes);
        _providers.ForEach(p => p.SetInterval(interval));
    }

    private void OnCloseClicked()
    {
        switch (_settings.CloseAction)
        {
            case CloseAction.HidePanel:
                HideByUser();
                break;
            case CloseAction.Quit:
                _desktop.Shutdown();
                break;
            default:
                _panel.ShowClosePrompt();
                break;
        }
    }

    /// <summary>Hides the panel until it is shown again from the tray menu or the settings.</summary>
    private void HideByUser()
    {
        _hiddenByUser = true;
        HidePanel();
        BuildTray();
    }

    private void ShowAgain()
    {
        _hiddenByUser = false;
        BuildTray();
        ApplyDisplayMode();
    }

    // ----- ISettingsHost -----

    public AppSettings Settings => _settings;

    public Strings Strings => _strings;

    public bool ClaudeConnected => ClaudeIntegration.IsInstalled();

    public bool StartAtLoginSupported => OperatingSystem.IsWindows();

    public bool StartAtLogin
    {
        get => OperatingSystem.IsWindows() && WindowsStartup.IsEnabled();
        set
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsStartup.SetEnabled(value);
            }
            BuildTray();
        }
    }

    public void SetDisplayMode(DisplayMode mode)
    {
        _settings.DisplayMode = mode;
        _settings.Save();
        _hiddenByUser = false; // Choosing a mode shows the panel again.
        BuildTray();
        ApplyDisplayMode();
    }

    public void SetRefresh(RefreshPreset preset, int customMinutes)
    {
        _settings.RefreshPreset = preset;
        _settings.CustomRefreshMinutes = RefreshInterval.ClampCustom(customMinutes);
        _settings.Save();
        ApplyRefreshInterval();
    }

    public void SetCloseAction(CloseAction action)
    {
        _settings.CloseAction = action;
        _settings.Save();
    }

    public void SetWidgetTopmost(bool topmost)
    {
        _settings.WidgetTopmost = topmost;
        _settings.Save();
        if (_settings.DisplayMode == DisplayMode.Widget)
        {
            _panel.Topmost = topmost;
        }
    }

    public IReadOnlyList<SectionSetting> Sections => _sections;

    public void SetSectionVisible(SectionId id, bool visible)
    {
        _sections.First(s => s.Id == id).Visible = visible;
        if (id == SectionId.Ime)
        {
            _imeLabel = null;
        }
        ApplySections();
    }

    public void MoveSection(SectionId id, int delta)
    {
        if (SectionLayout.Move(_sections, id, delta))
        {
            ApplySections();
        }
    }

    public void SetShowSeconds(bool show)
    {
        _settings.ShowSeconds = show;
        _settings.Save();
        RenderClock();
        if (_settings.DisplayMode == DisplayMode.Widget && !_hiddenByUser)
        {
            ShowWidget();
        }
    }

    public void SetUsageDisplay(UsageDisplay display)
    {
        _settings.UsageDisplay = display;
        _settings.Save();
        Render();
    }

    public void SetLanguage(string code)
    {
        _settings.Language = code;
        _settings.Save();
        _strings = Strings.For(code);
        BuildTray();
        Render();
    }

    public IReadOnlyList<string> AntigravityGroups => _antigravity.GroupNames;

    public string? AntigravityShownGroup => _antigravity.ShownGroup;

    public void SetAntigravityGroup(string group)
    {
        _settings.AntigravityGroup = group;
        _settings.Save();
        _antigravity.SelectGroup(group);
    }

    public string? SetClaudeConnected(bool connected)
    {
        string? error;
        bool ok = connected ? ClaudeIntegration.Install(_settings, out error) : ClaudeIntegration.Uninstall(_settings, out error);
        _providers.First(p => p.Id == ProviderId.ClaudeCode).Refresh();
        BuildTray();
        return ok ? null : error;
    }

    // ----- Tray and windows -----

    public void OpenSettings()
    {
        if (_settingsWindow is { } open)
        {
            open.Activate();
            return;
        }
        _settingsWindow = new SettingsWindow(this) { Icon = LoadIcon("app.png") };
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private void BuildTray()
    {
        var menu = new NativeMenu();

        if (_hiddenByUser)
        {
            var show = new NativeMenuItem(_strings.MenuShowPanel);
            show.Click += (_, _) => ShowAgain();
            menu.Items.Add(show);
        }

        var settings = new NativeMenuItem(_strings.MenuSettings);
        settings.Click += (_, _) => OpenSettings();
        menu.Items.Add(settings);
        menu.Items.Add(new NativeMenuItemSeparator());

        var language = new NativeMenuItem(_strings.MenuLanguage) { Menu = new NativeMenu() };
        foreach (var (code, label) in new[] { ("system", _strings.MenuSystemLanguage), ("en", "English"), ("ko", "한국어") })
        {
            var item = new NativeMenuItem(label)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                IsChecked = _settings.Language == code,
            };
            item.Click += (_, _) =>
            {
                SetLanguage(code);
                _settingsWindow?.Build();
            };
            language.Menu.Items.Add(item);
        }
        menu.Items.Add(language);

        bool claudeConnected = ClaudeIntegration.IsInstalled();
        var claude = new NativeMenuItem(claudeConnected ? _strings.MenuDisconnectClaude : _strings.MenuConnectClaude);
        claude.Click += (_, _) =>
        {
            if (SetClaudeConnected(!claudeConnected) is { } error && _tray is not null)
            {
                _tray.ToolTipText = string.Format(_strings.ClaudeConnectFailed, error);
            }
            _settingsWindow?.Build();
        };
        menu.Items.Add(claude);

        var refresh = new NativeMenuItem(_strings.MenuRefresh);
        refresh.Click += (_, _) => RefreshAll();
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
            _tray = new TrayIcon
            {
                Icon = LoadIcon("tray.png"),
                ToolTipText = $"{AppInfo.Name} {AppInfo.Version}",
                IsVisible = true,
            };
            _tray.Clicked += (_, _) => OpenSettings();
            TrayIcon.SetIcons(_app, [_tray]);
        }
        _tray.Menu = menu;
    }

    private static WindowIcon LoadIcon(string name)
    {
        using Stream stream = AssetLoader.Open(new Uri($"avares://AIUsageWidget/Assets/{name}"));
        return new WindowIcon(stream);
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
        _secondTimer.Stop();
        if (_saveWidgetTimer.IsEnabled)
        {
            SaveWidgetPosition();
        }
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
