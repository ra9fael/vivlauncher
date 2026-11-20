using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace VivLauncher.App;

public sealed class HighlightTextBlock : TextBlock
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Highlight), typeof(HighlightTextBlock), new PropertyMetadata(null, Changed));
    public Highlight? Value { get => (Highlight?)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    private static void Changed(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        var block = (HighlightTextBlock)target;
        block.Inlines.Clear();
        if (args.NewValue is not Highlight value) return;
        for (var index = 0; index < value.Text.Length;)
        {
            var highlighted = value.Positions.Contains(index);
            var end = index + 1;
            while (end < value.Text.Length && value.Positions.Contains(end) == highlighted) end++;
            var run = new Run(value.Text.Substring(index, end - index));
            if (highlighted)
            {
                run.SetResourceReference(TextElement.BackgroundProperty, "MatchSurface");
                run.SetResourceReference(TextElement.ForegroundProperty, "MatchInk");
                run.FontWeight = FontWeights.SemiBold;
            }
            block.Inlines.Add(run);
            index = end;
        }
    }
}

public sealed class InverseVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class BoolInverter : IValueConverter
{
    public static BoolInverter Instance { get; } = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not true;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is not true;
}

public sealed class ListWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Math.Max(100, (double)value - 24);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
