using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AIUsageWidget.Ime;

namespace AIUsageWidget.Views;

/// <summary>The keyboard input state of the window in front, such as "EN" or "한".</summary>
public sealed class ImeCell : Border
{
    private static readonly IBrush HangulBrush = new SolidColorBrush(Color.Parse("#F0B44C"));
    private static readonly IBrush OtherBrush = new SolidColorBrush(Color.Parse("#66A0F0"));
    private static readonly IBrush UnknownBrush = new SolidColorBrush(Color.Parse("#8A909C"));

    private readonly TextBlock _title;
    private readonly TextBlock _state;

    public ImeCell()
    {
        Width = 46;
        Padding = new Thickness(0, 2);
        CornerRadius = new CornerRadius(6);
        Background = new SolidColorBrush(Color.Parse("#232730"));
        VerticalAlignment = VerticalAlignment.Center;

        _title = new TextBlock
        {
            FontSize = 9,
            Foreground = UnknownBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _state = new TextBlock
        {
            Text = "–",
            FontSize = 17,
            FontWeight = FontWeight.Bold,
            Foreground = UnknownBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        Child = new StackPanel { Children = { _title, _state } };
    }

    public void Render(string? label, string title)
    {
        _title.Text = title;
        _state.Text = label ?? "–";
        _state.Foreground = label switch
        {
            null => UnknownBrush,
            ImeLabel.Hangul => HangulBrush,
            _ => OtherBrush,
        };
    }
}
