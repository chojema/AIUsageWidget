using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AIUsageWidget.Core;
using AIUsageWidget.Ime;
using AIUsageWidget.Localization;

namespace AIUsageWidget.Views;

public sealed partial class PanelWindow : Window
{
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromSeconds(8);

    private readonly Dictionary<ProviderId, ProviderCell> _providers = new();
    private readonly Dictionary<SectionId, BadgeCell> _badges = new()
    {
        [SectionId.Date] = new BadgeCell(64),
        [SectionId.Weekday] = new BadgeCell(44),
        [SectionId.Time] = new BadgeCell(56),
        [SectionId.Ime] = new BadgeCell(44),
    };
    private readonly Grid _cells;
    private readonly StackPanel _buttons;
    private readonly Border _frame;
    private readonly Border _closePrompt;
    private readonly TextBlock _closePromptText;
    private readonly Button _closeButton;
    private readonly Button _refreshButton;
    private readonly Button _hideButton;
    private readonly Button _quitButton;
    private readonly Button _cancelButton;
    private readonly DispatcherTimer _promptTimer;
    private readonly MenuItem _settingsMenuItem = new();
    private readonly MenuItem _refreshMenuItem = new();
    private readonly MenuItem _hideMenuItem = new();
    private readonly MenuItem _quitMenuItem = new();

    public PanelWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _frame = this.FindControl<Border>("Frame")!;
        _cells = this.FindControl<Grid>("Cells")!;
        _buttons = this.FindControl<StackPanel>("Buttons")!;
        _closePrompt = this.FindControl<Border>("ClosePrompt")!;
        _closePromptText = this.FindControl<TextBlock>("ClosePromptText")!;
        _closeButton = this.FindControl<Button>("CloseButton")!;
        _refreshButton = this.FindControl<Button>("RefreshButton")!;
        _hideButton = this.FindControl<Button>("HideButton")!;
        _quitButton = this.FindControl<Button>("QuitButton")!;
        _cancelButton = this.FindControl<Button>("CancelButton")!;

        foreach (ProviderId id in Enum.GetValues<ProviderId>())
        {
            _providers[id] = new ProviderCell(id);
        }
        ApplyLayout(SectionLayout.DefaultOrder);

        _promptTimer = new DispatcherTimer(PromptTimeout, DispatcherPriority.Background, (_, _) => HideClosePrompt());
        _promptTimer.Stop();

        _closeButton.Click += (_, _) => CloseClicked?.Invoke();
        _refreshButton.Click += (_, _) => RefreshClicked?.Invoke();
        _hideButton.Click += (_, _) =>
        {
            HideClosePrompt();
            HidePanelChosen?.Invoke();
        };
        _quitButton.Click += (_, _) =>
        {
            HideClosePrompt();
            QuitChosen?.Invoke();
        };
        _cancelButton.Click += (_, _) => HideClosePrompt();

        ContextMenu = new ContextMenu
        {
            ItemsSource = new Control[]
            {
                _settingsMenuItem, _refreshMenuItem, new Separator(), _hideMenuItem, _quitMenuItem,
            },
        };
        ContextMenu.Opening += (_, _) => HideClosePrompt();
        _settingsMenuItem.Click += (_, _) => InvokeMenuAction(() => SettingsClicked?.Invoke());
        _refreshMenuItem.Click += (_, _) => InvokeMenuAction(() => RefreshClicked?.Invoke());
        _hideMenuItem.Click += (_, _) => InvokeMenuAction(() => HidePanelChosen?.Invoke());
        _quitMenuItem.Click += (_, _) => InvokeMenuAction(() => QuitChosen?.Invoke());

        PointerPressed += OnPointerPressed;
    }

    public event Action? SettingsClicked;

    public bool IsContextMenuOpen => ContextMenu?.IsOpen == true;

    /// <summary>The ✕ button was clicked.</summary>
    public event Action? CloseClicked;

    /// <summary>The ↻ button was clicked.</summary>
    public event Action? RefreshClicked;

    /// <summary>"Hide panel" was chosen in the close prompt.</summary>
    public event Action? HidePanelChosen;

    /// <summary>"Quit" was chosen in the close prompt.</summary>
    public event Action? QuitChosen;

    /// <summary>True while the panel is a free-standing widget that the user can drag.</summary>
    public bool Draggable
    {
        get;
        set
        {
            field = value;
            // A widget gets a full border and rounded corners; the attached panel only a top line.
            _frame.BorderThickness = value ? new Thickness(1) : new Thickness(0, 1, 0, 0);
            _frame.CornerRadius = value ? new CornerRadius(8) : default;
            Cursor = value ? new Cursor(StandardCursorType.SizeAll) : Cursor.Default;
        }
    }

    /// <summary>Lays out the visible sections in the given order, followed by the buttons.</summary>
    public void ApplyLayout(IReadOnlyList<SectionId> visible)
    {
        _cells.Children.Clear();
        _cells.ColumnDefinitions.Clear();

        SectionId? previous = null;
        foreach (SectionId id in visible)
        {
            if (previous is { } p)
            {
                // Wider gaps between usage sections, narrower ones around the small badges.
                bool bothProviders = SectionLayout.IsProvider(p) && SectionLayout.IsProvider(id);
                _cells.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(bothProviders ? 16 : 8)));
            }

            Control cell = SectionLayout.ProviderOf(id) is { } provider ? _providers[provider] : _badges[id];
            _cells.ColumnDefinitions.Add(new ColumnDefinition(
                SectionLayout.IsProvider(id) ? new GridLength(1, GridUnitType.Star) : GridLength.Auto));
            Grid.SetColumn(cell, _cells.ColumnDefinitions.Count - 1);
            _cells.Children.Add(cell);
            previous = id;
        }

        if (!visible.Any(SectionLayout.IsProvider))
        {
            // Without a usage section, push the badges and buttons to the right edge.
            _cells.ColumnDefinitions.Insert(0, new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            foreach (Control child in _cells.Children)
            {
                Grid.SetColumn(child, Grid.GetColumn(child) + 1);
            }
        }

        _cells.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(6)));
        _cells.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        Grid.SetColumn(_buttons, _cells.ColumnDefinitions.Count - 1);
        _cells.Children.Add(_buttons);
    }

    public void Render(IEnumerable<ProviderSnapshot> snapshots, Strings strings, DateTimeOffset now, UsageDisplay display)
    {
        foreach (ProviderSnapshot snapshot in snapshots)
        {
            if (_providers.TryGetValue(snapshot.Provider, out ProviderCell? cell))
            {
                cell.Render(snapshot, strings, now, display);
            }
        }

        ToolTip.SetTip(_closeButton, strings.CloseTooltip);
        ToolTip.SetTip(_refreshButton, strings.MenuRefresh);
        _closePromptText.Text = strings.ClosePrompt;
        _hideButton.Content = strings.CloseHideButton;
        _quitButton.Content = strings.CloseQuitButton;
        _cancelButton.Content = strings.Cancel;
        _settingsMenuItem.Header = strings.MenuSettings;
        _refreshMenuItem.Header = strings.MenuRefresh;
        _hideMenuItem.Header = strings.MenuHidePanel;
        _quitMenuItem.Header = strings.CloseQuitButton;
    }

    public void RenderClock(DateTimeOffset now, Strings strings, bool seconds)
    {
        _badges[SectionId.Date].Render(strings.DateTitle, UsageFormatter.Date(now, strings));
        _badges[SectionId.Weekday].Render(strings.WeekdayTitle, UsageFormatter.Weekday(now, strings),
            now.ToLocalTime().DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? BadgeCell.Accent : BadgeCell.Neutral);
        _badges[SectionId.Time].Render(strings.TimeTitle, UsageFormatter.Time(now, seconds));
    }

    public void RenderIme(string? label, Strings strings) =>
        _badges[SectionId.Ime].Render(strings.ImeTitle, label, label == ImeLabel.Hangul ? BadgeCell.Accent : BadgeCell.Info);

    /// <summary>Shows the choice between hiding the panel and quitting, inside the panel itself.</summary>
    public void ShowClosePrompt()
    {
        _closePrompt.IsVisible = true;
        _promptTimer.Stop();
        _promptTimer.Start();
    }

    public void HideClosePrompt()
    {
        _promptTimer.Stop();
        _closePrompt.IsVisible = false;
    }

    private void InvokeMenuAction(Action action)
    {
        ContextMenu?.Close();
        HideClosePrompt();
        action();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Draggable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
            (e.Source as Visual)?.FindAncestorOfType<Button>(includeSelf: true) is null)
        {
            BeginMoveDrag(e);
        }
    }
}
