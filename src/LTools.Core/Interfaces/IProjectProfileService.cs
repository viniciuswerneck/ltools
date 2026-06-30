namespace LTools.Core.Interfaces;

/// <summary>Per-project configuration storage. Keys are automatically scoped by project path.</summary>
public interface IProjectProfileService
{
    /// <summary>Retrieves a project-scoped setting.</summary>
    T? Get<T>(string key);
    /// <summary>Saves a project-scoped setting.</summary>
    void Set<T>(string key, T value);
    /// <summary>Gets or sets the last active plugin for this project.</summary>
    string? LastPlugin { get; set; }
    /// <summary>Gets or sets the last active plugin for this project.</summary>
    string? LastEnvFile { get; set; }
    /// <summary>Gets or sets the last active plugin for this project.</summary>
    string? LastDatabaseConnection { get; set; }
}
