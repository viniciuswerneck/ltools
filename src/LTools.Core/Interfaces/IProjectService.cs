namespace LTools.Core.Interfaces;

/// <summary>Manages the currently open Laravel project context.</summary>
public interface IProjectService
{
    /// <summary>Full path to the current project directory.</summary>
    string? CurrentPath { get; set; }
    /// <summary>Project folder name extracted from the path.</summary>
    string? CurrentName { get; }
    /// <summary>Whether a project is currently loaded.</summary>
    bool HasProject { get; }
    /// <summary>Raised when the current project changes.</summary>
    event Action? ProjectChanged;
    /// <summary>Checks whether the given path points to a valid Laravel project.</summary>
    bool IsLaravelProject(string path);
}