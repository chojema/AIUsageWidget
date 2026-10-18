using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AIUsageWidget.Core;
using AIUsageWidget.Localization;

namespace AIUsageWidget.Views;

/// <summary>One provider: a title line and one line for each limit window.</summary>
public sealed class ProviderCell : Grid
{
    private const double FontSizeText = 11;
    private const double BarWidth = 40;

    private static readonly IBrush Primary = new SolidColorBrush(Color.Parse("#E6E8EC"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#8A909C"));
    private static readonly IBrush Track = new SolidColorBrush(Color.Parse("#343944"));
    private static readonly IBrush Good = new SolidColorBrush(Color.Parse("#5EC488"));
    private static readonly IBrush Warn = new SolidColorBrush(Color.Parse("#F0B44C"));
    private static readonly IBrush Low = new SolidColorBrush(Color.Parse("#E86A5E"));

    private readonly TextBlock _status;
    private readonly Row _fiveHour;
    private readonly Row _weekly;

    public ProviderCell(ProviderId provider)
    {
        RowDefinitions = new RowDefinitions("Auto,Auto,Auto");
        VerticalAlignment = VerticalAlignment.Center;

        var title = new DockPanel();
        title.Children.Add(Text(Strings.ProviderName(provider), Primary, FontWeight.SemiBold));
        _status = Text(string.Empty, Muted);
        _status.HorizontalAlignment = HorizontalAlignment.Right;
        _status.Margin = new Thickness(8, 0, 0, 0);
        DockPanel.SetDock(_status, Dock.Right);
        title.Children.Insert(0, _status);
        Children.Add(title);

        _fiveHour = new Row();
        SetRow(_fiveHour.Panel, 1);
        Children.Add(_fiveHour.Panel);

        _weekly = new Row();
        SetRow(_weekly.Panel, 2);
        Children.Add(_weekly.Panel);
    }

    public void Render(ProviderSnapshot snapshot, Strings s, DateTimeOffset now)
    {
        if (snapshot.State != ProviderState.Ok)
        {
            _status.Text = string.Empty;
            _fiveHour.ShowMessage(s.StateText(snapshot));
            _weekly.ShowMessage(string.Empty);
            return;
        }

        _status.Text = snapshot.UpdatedAt is { } updated && snapshot.Provider == ProviderId.ClaudeCode
            ? UsageFormatter.Updated(updated, now, s)
            : string.Empty;
        _fiveHour.Show(s.FiveHour, snapshot.FiveHour, s, now);
        _weekly.Show(s.Weekly, snapshot.Weekly, s, now);
    }

    private static TextBlock Text(string text, IBrush brush, FontWeight weight = FontWeight.Normal) => new()
    {
        Text = text,
        FontSize = FontSizeText,
        Foreground = brush,
        FontWeight = weight,
        TextTrimming = TextTrimming.CharacterEllipsis,
        VerticalAlignment = VerticalAlignment.Center,
    };

    private sealed class Row
    {
        private readonly TextBlock _label = Text(string.Empty, Muted);
        private readonly Border _fill = new() { Height = 4, CornerRadius = new CornerRadius(2), HorizontalAlignment = HorizontalAlignment.Left };
        private readonly Border _track;
        private readonly TextBlock _value = Text(string.Empty, Primary);

        public Row()
        {
            _label.Width = 38;
            _track = new Border
            {
                Width = BarWidth,
                Height = 4,
                CornerRadius = new CornerRadius(2),
                Background = Track,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = _fill,
            };
            Panel = new DockPanel { Height = 14 };
            DockPanel.SetDock(_label, Dock.Left);
            DockPanel.SetDock(_track, Dock.Left);
            Panel.Children.Add(_label);
            Panel.Children.Add(_track);
            Panel.Children.Add(_value);
        }

        public DockPanel Panel { get; }

        public void Show(string label, UsageWindow? window, Strings s, DateTimeOffset now)
        {
            _label.Text = label;
            _label.IsVisible = true;
            _track.IsVisible = true;
            if (window is null)
            {
                _fill.Width = 0;
                _value.Text = "—";
                _value.Foreground = Muted;
                return;
            }

            double remaining = window.RemainingPercent;
            _fill.Width = BarWidth * remaining / 100.0;
            _fill.Background = remaining > 50 ? Good : remaining > 20 ? Warn : Low;
            string reset = UsageFormatter.Reset(window, now, s);
            _value.Text = reset.Length > 0 ? $"{UsageFormatter.Remaining(window, s)} · {reset}" : UsageFormatter.Remaining(window, s);
            _value.Foreground = Primary;
        }

        public void ShowMessage(string message)
        {
            _label.IsVisible = false;
            _track.IsVisible = false;
            _value.Text = message;
            _value.Foreground = Muted;
        }
    }
}
