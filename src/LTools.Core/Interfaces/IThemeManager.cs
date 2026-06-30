namespace LTools.Core.Interfaces;

/// <summary>Application theme options.</summary>
public enum ThemeVariant
{
    /// <summary>Light theme.</summary>
    Light,
    /// <summary>Dark theme.</summary>
    Dark,
    /// <summary>Follow the system theme setting.</summary>
    System
}

/// <summary>Manages light/dark/system theme switching.</summary>
public interface IThemeManager
{
    /// <summary>Currently active theme.</summary>
    ThemeVariant CurrentTheme { get; }
    /// <summary>Raised when the theme changes.</summary>
    event Action<ThemeVariant>? ThemeChanged;
    /// <summary>Applies the specified theme.</summary>
    void SetTheme(ThemeVariant theme);
}
