namespace LTools.Core.Interfaces;

public enum ThemeVariant
{
    Light,
    Dark,
    System
}

public interface IThemeManager
{
    ThemeVariant CurrentTheme { get; }
    event Action<ThemeVariant>? ThemeChanged;
    void SetTheme(ThemeVariant theme);
}
