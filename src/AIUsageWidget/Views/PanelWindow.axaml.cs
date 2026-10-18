using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AIUsageWidget.Core;
using AIUsageWidget.Localization;

namespace AIUsageWidget.Views;

public sealed partial class PanelWindow : Window
{
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromSeconds(8);

    private readonly Dictionary<ProviderId, ProviderCell> _cells = new();
    private readonly ImeCell _ime = new();
    private readonly Border _frame;
    private readonly Border _closePrompt;
    private readonly TextBlock _closePromptText;
    private readonly Button _closeButton;
    private readonly Button _refreshButton;
    private readonly Button _hideButton;
    private readonly Button _quitButton;
    private readonly Button _cancelButton;
    private readonly DispatcherTimer _promptTimer;

    public PanelWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _frame = this.FindControl<Border>("Frame")!;
        _closePrompt = this.FindControl<Border>("ClosePrompt")!;
        _closePromptText = this.FindControl<TextBlock>("ClosePromptText")!;
        _closeButton = this.FindControl<Button>("CloseButton")!;
        _refreshButton = this.FindControl<Button>("RefreshButton")!;
        _hideButton = this.FindControl<Button>("HideButton")!;
        _quitButton = this.FindControl<Button>("QuitButton")!;
        _cancelButton = this.FindControl<Button>("CancelButton")!;

        Grid cells = this.FindControl<Grid>("Cells")!;
        ProviderId[] order = [ProviderId.Codex, ProviderId.ClaudeCode, ProviderId.Antigravity];
        for (int i = 0; i < order.Length; i++)
        {
            var cell = new ProviderCell(order[i]);
            Grid.SetColumn(cell, i * 2);
            cells.Children.Add(cell);
            _cells[order[i]] = cell;
        }
        Grid.SetColumn(_ime, 6);
        cells.Children.Add(_ime);

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

        PointerPressed += OnPointerPressed;
    }

    /// <summary>The ✕ button was clicked.</summary>
    public event Action? CloseClicked;

    /// <summary>The ⟳ button was clicked.</summary>
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

    public void Render(IEnumerable<ProviderSnapshot> snapshots, Strings strings, DateTimeOffset now)
    {
        foreach (ProviderSnapshot snapshot in snapshots)
        {
            if (_cells.TryGetValue(snapshot.Provider, out ProviderCell? cell))
            {
                cell.Render(snapshot, strings, now);
            }
        }

        ToolTip.SetTip(_closeButton, strings.CloseTooltip);
        ToolTip.SetTip(_refreshButton, strings.MenuRefresh);
        _closePromptText.Text = strings.ClosePrompt;
        _hideButton.Content = strings.CloseHideButton;
        _quitButton.Content = strings.CloseQuitButton;
        _cancelButton.Content = strings.Cancel;
    }

    public void RenderIme(bool visible, string? label, Strings strings)
    {
        _ime.IsVisible = visible;
        if (visible)
        {
            _ime.Render(label, strings.ImeTitle);
        }
    }

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

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Draggable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && (e.Source as Visual)?.FindAncestorOfType<Button>(includeSelf: true) is null)
        {
            BeginMoveDrag(e);
        }
    }
}
