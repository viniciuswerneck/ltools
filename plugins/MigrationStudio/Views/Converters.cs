using System.Globalization;
using Avalonia.Data.Converters;

namespace LTools.MigrationStudio.Views;

public class IsNotNullConverter : IValueConverter
{
    public static readonly IsNotNullConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class IsNullConverter : IValueConverter
{
    public static readonly IsNullConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value == null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class TypeShowLengthConverter : IValueConverter
{
    public static readonly TypeShowLengthConverter Instance = new();

    private static readonly HashSet<string> TypesWithLength = ["string", "char"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string type && TypesWithLength.Contains(type);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class TypeShowEnumConverter : IValueConverter
{
    public static readonly TypeShowEnumConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string type && type == "enum";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class TypeShowPrecisionConverter : IValueConverter
{
    public static readonly TypeShowPrecisionConverter Instance = new();

    private static readonly HashSet<string> TypesWithPrecision = ["decimal", "float", "double"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string type && TypesWithPrecision.Contains(type);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class HasTextConverter : IValueConverter
{
    public static readonly HasTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string text && !string.IsNullOrWhiteSpace(text);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
