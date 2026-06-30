using LTools.Core.Models;

namespace LTools.Core.Interfaces;

/// <summary>Scans the filesystem for Laravel projects.</summary>
public interface ILaravelDetector
{
    /// <summary>Recursively scans a directory for Laravel projects.</summary>
    Task<List<LaravelProject>> ScanAsync(string rootPath);
    /// <summary>Checks whether a specific path is a Laravel project and returns its metadata.</summary>
    Task<LaravelProject?> DetectAsync(string projectPath);
    /// <summary>Quick check whether the given path is a Laravel project.</summary>
    bool IsLaravelProject(string path);
}
