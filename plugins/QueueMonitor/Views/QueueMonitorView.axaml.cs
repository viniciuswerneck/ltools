using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LTools.QueueMonitor.Views;

public partial class QueueMonitorView : UserControl
{
    public QueueMonitorView()
    {
        InitializeComponent();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return true;
    }
}

public class BoolToTabBgConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool active && active)
            return new SolidColorBrush(Color.Parse("#333333"));
        return new SolidColorBrush(Color.Parse("#1A1A1A"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString() ?? "";
        return status switch
        {
            "empty" => new SolidColorBrush(Color.Parse("#555555")),
            "running" => new SolidColorBrush(Color.Parse("#33AA33")),
            "paused" => new SolidColorBrush(Color.Parse("#AAAA33")),
            _ => new SolidColorBrush(Color.Parse("#555555"))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
