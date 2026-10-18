using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AIUsageWidget.Views;

/// <summary>A small section with a caption and one prominent value, such as the date or the input state.</summary>
public sealed class BadgeCell : Border
{
    public static readonly IBrush Neutral = new SolidColorBrush(Color.Parse("#E6E8EC"));
    public static readonly IBrush Accent = new SolidColorBrush(Color.Parse("#F0B44C"));
    public static readonly IBrush Info = new SolidColorBrush(Color.Parse("#66A0F0"));
    public static readonly IBrush Unknown = new SolidColorBrush(Color.Parse("#8A909C"));

    private readonly TextBlock _title;
    private readonly TextBlock _value;

    public BadgeCell(double minWidth)
    {
        MinWidth = minWidth;
        Padding = new Thickness(8, 2);
        CornerRadius = new CornerRadius(6);
        Background = new SolidColorBrush(Color.Parse("#232730"));
        VerticalAlignment = VerticalAlignment.Center;

        _title = new TextBlock
        {
            FontSize = 9,
            Foreground = Unknown,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _value = new TextBlock
        {
            Text = "–",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Neutral,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        Child = new StackPanel { Children = { _title, _value } };
    }

    public void Render(string title, string? value, IBrush? brush = null)
    {
        _title.Text = title;
        _value.Text = value ?? "–";
        _value.Foreground = value is null ? Unknown : brush ?? Neutral;
    }
}
