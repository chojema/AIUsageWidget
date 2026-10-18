using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AIUsageWidget.Core;
using AIUsageWidget.Localization;

namespace AIUsageWidget.Views;

public sealed partial class PanelWindow : Window
{
    private readonly Dictionary<ProviderId, ProviderCell> _cells = new();
    private readonly ImeCell _ime = new();
    private readonly Border _frame;

    public PanelWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _frame = this.FindControl<Border>("Frame")!;
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

        PointerPressed += OnPointerPressed;
    }

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
    }

    public void RenderIme(bool visible, string? label, Strings strings)
    {
        _ime.IsVisible = visible;
        if (visible)
        {
            _ime.Render(label, strings.ImeTitle);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Draggable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}
