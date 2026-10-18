using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AIUsageWidget.Core;
using AIUsageWidget.Localization;

namespace AIUsageWidget.Views;

/// <summary>Values the settings window can change. Each change is applied as soon as it is made.</summary>
public interface ISettingsHost
{
    AppSettings Settings { get; }
    Strings Strings { get; }
    bool ClaudeConnected { get; }
    bool StartAtLoginSupported { get; }
    bool StartAtLogin { get; set; }
    void SetDisplayMode(DisplayMode mode);
    void SetWidgetTopmost(bool topmost);
    void SetShowIme(bool show);
    void SetLanguage(string code);
    void SetRefresh(RefreshPreset preset, int customMinutes);
    void SetCloseAction(CloseAction action);
    /// <summary>Antigravity model groups from the last successful read.</summary>
    IReadOnlyList<string> AntigravityGroups { get; }
    /// <summary>The group shown in the panel after any fallback.</summary>
    string? AntigravityShownGroup { get; }
    void SetAntigravityGroup(string group);
    /// <summary>Returns an error message, or null on success.</summary>
    string? SetClaudeConnected(bool connected);
}

public sealed class SettingsWindow : Window
{
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#8A909C"));
    private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.Parse("#E86A5E"));

    private readonly ISettingsHost _host;
    private bool _building;

    public SettingsWindow(ISettingsHost host)
    {
        _host = host;
        Width = 480;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Build();
    }

    /// <summary>Rebuilds the content, for example after the language changed.</summary>
    public void Build()
    {
        _building = true;
        Strings s = _host.Strings;
        AppSettings settings = _host.Settings;
        Title = s.SettingsTitle;

        var root = new StackPanel { Margin = new Thickness(20), Spacing = 6 };

        // Display mode
        root.Children.Add(Heading(s.DisplayMode));
        var terminal = new RadioButton { GroupName = "mode", Content = s.ModeTerminal, IsChecked = settings.DisplayMode == DisplayMode.Terminal };
        var widget = new RadioButton { GroupName = "mode", Content = s.ModeWidget, IsChecked = settings.DisplayMode == DisplayMode.Widget };
        var topmost = new CheckBox
        {
            Content = s.WidgetTopmost,
            IsChecked = settings.WidgetTopmost,
            IsEnabled = settings.DisplayMode == DisplayMode.Widget,
            Margin = new Thickness(28, 0, 0, 0),
        };
        terminal.IsCheckedChanged += (_, _) =>
        {
            if (!_building && terminal.IsChecked == true)
            {
                _host.SetDisplayMode(DisplayMode.Terminal);
                topmost.IsEnabled = false;
            }
        };
        widget.IsCheckedChanged += (_, _) =>
        {
            if (!_building && widget.IsChecked == true)
            {
                _host.SetDisplayMode(DisplayMode.Widget);
                topmost.IsEnabled = true;
            }
        };
        topmost.IsCheckedChanged += (_, _) =>
        {
            if (!_building)
            {
                _host.SetWidgetTopmost(topmost.IsChecked == true);
            }
        };
        root.Children.Add(terminal);
        root.Children.Add(Hint(s.ModeTerminalHint));
        root.Children.Add(widget);
        root.Children.Add(Hint(s.ModeWidgetHint));
        root.Children.Add(topmost);

        // Refresh
        root.Children.Add(Heading(s.RefreshSection));
        RefreshPreset[] presets =
        [
            RefreshPreset.Manual, RefreshPreset.OneMinute, RefreshPreset.FiveMinutes,
            RefreshPreset.ThirtyMinutes, RefreshPreset.OneHour, RefreshPreset.Custom,
        ];
        var interval = new ComboBox
        {
            ItemsSource = new[]
            {
                s.RefreshManual, s.RefreshOneMinute, s.RefreshFiveMinutes,
                s.RefreshThirtyMinutes, s.RefreshOneHour, s.RefreshCustom,
            },
            SelectedIndex = Math.Max(0, Array.IndexOf(presets, settings.RefreshPreset)),
            MinWidth = 180,
        };
        var minutes = new NumericUpDown
        {
            Minimum = RefreshInterval.MinCustomMinutes,
            Maximum = RefreshInterval.MaxCustomMinutes,
            Increment = 1,
            FormatString = "0",
            Value = RefreshInterval.ClampCustom(settings.CustomRefreshMinutes),
            Width = 130,
            IsVisible = settings.RefreshPreset == RefreshPreset.Custom,
        };
        var minutesUnit = new TextBlock
        {
            Text = s.MinutesUnit,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = minutes.IsVisible,
        };
        var refreshHint = Hint(settings.RefreshPreset == RefreshPreset.Manual ? s.RefreshManualHint : s.RefreshHint);
        refreshHint.Margin = new Thickness(0, 2, 0, 0);

        void ApplyRefresh()
        {
            if (_building || interval.SelectedIndex < 0)
            {
                return;
            }
            RefreshPreset preset = presets[interval.SelectedIndex];
            int custom = RefreshInterval.ClampCustom((int)(minutes.Value ?? settings.CustomRefreshMinutes));
            minutes.IsVisible = minutesUnit.IsVisible = preset == RefreshPreset.Custom;
            refreshHint.Text = preset == RefreshPreset.Manual ? _host.Strings.RefreshManualHint : _host.Strings.RefreshHint;
            _host.SetRefresh(preset, custom);
        }
        interval.SelectionChanged += (_, _) => ApplyRefresh();
        minutes.ValueChanged += (_, _) => ApplyRefresh();

        var intervalRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        intervalRow.Children.Add(new TextBlock { Text = s.RefreshInterval, VerticalAlignment = VerticalAlignment.Center });
        intervalRow.Children.Add(interval);
        intervalRow.Children.Add(minutes);
        intervalRow.Children.Add(minutesUnit);
        root.Children.Add(intervalRow);
        root.Children.Add(refreshHint);

        // General
        root.Children.Add(Heading(s.General));
        CloseAction[] closeActions = [CloseAction.Ask, CloseAction.HidePanel, CloseAction.Quit];
        var closeAction = new ComboBox
        {
            ItemsSource = new[] { s.CloseAsk, s.CloseHide, s.CloseQuit },
            SelectedIndex = Math.Max(0, Array.IndexOf(closeActions, settings.CloseAction)),
            MinWidth = 180,
        };
        closeAction.SelectionChanged += (_, _) =>
        {
            if (!_building && closeAction.SelectedIndex >= 0)
            {
                _host.SetCloseAction(closeActions[closeAction.SelectedIndex]);
            }
        };
        var closeRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new Thickness(0, 0, 0, 4) };
        closeRow.Children.Add(new TextBlock { Text = s.CloseButtonAction, VerticalAlignment = VerticalAlignment.Center });
        closeRow.Children.Add(closeAction);
        root.Children.Add(closeRow);
        var showIme = new CheckBox { Content = s.ShowIme, IsChecked = settings.ShowIme };
        showIme.IsCheckedChanged += (_, _) =>
        {
            if (!_building)
            {
                _host.SetShowIme(showIme.IsChecked == true);
            }
        };
        root.Children.Add(showIme);

        if (_host.StartAtLoginSupported)
        {
            var startAtLogin = new CheckBox { Content = s.MenuStartAtLogin, IsChecked = _host.StartAtLogin };
            startAtLogin.IsCheckedChanged += (_, _) =>
            {
                if (!_building)
                {
                    _host.StartAtLogin = startAtLogin.IsChecked == true;
                }
            };
            root.Children.Add(startAtLogin);
        }

        string[] codes = ["system", "en", "ko"];
        var language = new ComboBox
        {
            ItemsSource = new[] { s.MenuSystemLanguage, "English", "한국어" },
            SelectedIndex = Math.Max(0, Array.IndexOf(codes, settings.Language)),
            MinWidth = 180,
        };
        language.SelectionChanged += (_, _) =>
        {
            if (!_building && language.SelectedIndex >= 0)
            {
                _host.SetLanguage(codes[language.SelectedIndex]);
                Build();
            }
        };
        var languageRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new Thickness(0, 4, 0, 0) };
        languageRow.Children.Add(new TextBlock { Text = s.MenuLanguage, VerticalAlignment = VerticalAlignment.Center });
        languageRow.Children.Add(language);
        root.Children.Add(languageRow);

        // Antigravity
        root.Children.Add(Heading(s.AntigravitySection));
        IReadOnlyList<string> groups = _host.AntigravityGroups;
        if (groups.Count == 0)
        {
            var pending = Hint(s.AntigravityGroupsPending);
            pending.Margin = new Thickness(0);
            root.Children.Add(pending);
        }
        else
        {
            var groupBox = new ComboBox
            {
                ItemsSource = groups,
                SelectedItem = _host.AntigravityShownGroup,
                MinWidth = 220,
            };
            groupBox.SelectionChanged += (_, _) =>
            {
                if (!_building && groupBox.SelectedItem is string group)
                {
                    _host.SetAntigravityGroup(group);
                }
            };
            var groupRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            groupRow.Children.Add(new TextBlock { Text = s.AntigravityGroup, VerticalAlignment = VerticalAlignment.Center });
            groupRow.Children.Add(groupBox);
            root.Children.Add(groupRow);
            var groupHint = Hint(s.AntigravityGroupHint);
            groupHint.Margin = new Thickness(0, 2, 0, 0);
            root.Children.Add(groupHint);
        }

        // Claude Code
        root.Children.Add(Heading(s.ClaudeSection));
        bool connected = _host.ClaudeConnected;
        var claudeStatus = Hint(connected ? s.ClaudeConnected : s.ClaudeDisconnected);
        claudeStatus.Margin = new Thickness(0);
        var claudeButton = new Button { Content = connected ? s.Disconnect : s.Connect };
        claudeButton.Click += (_, _) =>
        {
            string? error = _host.SetClaudeConnected(!_host.ClaudeConnected);
            Build();
            if (error is not null && Content is StackPanel panel)
            {
                panel.Children.Insert(panel.Children.Count - 1, new TextBlock
                {
                    Text = string.Format(_host.Strings.ClaudeConnectFailed, error),
                    Foreground = ErrorBrush,
                    TextWrapping = TextWrapping.Wrap,
                });
            }
        };
        root.Children.Add(claudeStatus);
        root.Children.Add(claudeButton);

        var close = new Button
        {
            Content = s.Close,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0),
            MinWidth = 80,
        };
        close.Click += (_, _) => Close();
        root.Children.Add(close);

        Content = root;
        _building = false;
    }

    private static TextBlock Heading(string text) => new()
    {
        Text = text,
        FontWeight = FontWeight.SemiBold,
        FontSize = 14,
        Margin = new Thickness(0, 10, 0, 2),
    };

    private static TextBlock Hint(string text) => new()
    {
        Text = text,
        Foreground = Muted,
        FontSize = 12,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(28, -4, 0, 4),
    };
}
