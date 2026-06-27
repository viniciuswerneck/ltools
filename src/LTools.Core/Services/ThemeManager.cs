using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ThemeManager : IThemeManager
{
    public ThemeVariant CurrentTheme { get; private set; } = ThemeVariant.System;

    public event Action<ThemeVariant>? ThemeChanged;

    public void SetTheme(ThemeVariant theme)
    {
        CurrentTheme = theme;
        ThemeChanged?.Invoke(theme);
    }
}
