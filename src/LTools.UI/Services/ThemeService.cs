using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LTools.Core.Interfaces;

namespace LTools.UI.Services;

public sealed class ThemeService : IThemeManager
{
    public ThemeVariant CurrentTheme { get; private set; } = ThemeVariant.Dark;
    public event Action<ThemeVariant>? ThemeChanged;

    public void Initialize()
    {
        var uri = new Uri("avares://LTools.UI/Styles/Colors.axaml");
        if (AvaloniaXamlLoader.Load(uri) is ResourceDictionary colorsDict)
            Application.Current!.Resources.MergedDictionaries.Insert(0, colorsDict);
        ApplyTheme(CurrentTheme);
    }

    public void SetTheme(ThemeVariant theme)
    {
        if (theme == CurrentTheme) return;
        CurrentTheme = theme;
        ApplyTheme(theme);
        ThemeChanged?.Invoke(theme);
    }

    public void Toggle()
    {
        SetTheme(CurrentTheme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark);
    }

    private static void ApplyTheme(ThemeVariant theme)
    {
        Application.Current!.RequestedThemeVariant = theme switch
        {
            ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
            _ => Avalonia.Styling.ThemeVariant.Dark
        };
    }
}
