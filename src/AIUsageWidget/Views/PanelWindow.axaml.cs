using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AIUsageWidget.Core;
using AIUsageWidget.Localization;

namespace AIUsageWidget.Views;

public sealed partial class PanelWindow : Window
{
    private readonly Dictionary<ProviderId, ProviderCell> _cells = new();

    public PanelWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Grid cells = this.FindControl<Grid>("Cells")!;
        ProviderId[] order = [ProviderId.Codex, ProviderId.ClaudeCode, ProviderId.Antigravity];
        for (int i = 0; i < order.Length; i++)
        {
            var cell = new ProviderCell(order[i]);
            Grid.SetColumn(cell, i * 2);
            cells.Children.Add(cell);
            _cells[order[i]] = cell;
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
}
