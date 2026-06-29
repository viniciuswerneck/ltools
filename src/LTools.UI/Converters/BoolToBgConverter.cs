using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LTools.UI.Converters;

public class BoolToBgConverter : IValueConverter
{
    public static readonly BoolToBgConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            if (Application.Current?.TryGetResource("AccentBg", Application.Current.RequestedThemeVariant, out var accentBg) == true
                && accentBg is Color color)
                return new SolidColorBrush(color);
            return new SolidColorBrush(Color.FromArgb(26, 0, 120, 212));
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
