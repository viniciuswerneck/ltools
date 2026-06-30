using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ProjectService : IProjectService
{
    private readonly IConfigManager _config;
    private string? _currentPath;

    public ProjectService(IConfigManager config)
    {
        _config = config;
        var lastPath = _config.Get<string>("last_project_path");
        if (!string.IsNullOrWhiteSpace(lastPath) && Directory.Exists(lastPath))
            _currentPath = lastPath;
    }

    public string? CurrentPath
    {
        get => _currentPath;
        set
        {
            if (_currentPath != value)
            {
                _currentPath = value;
                _config.Set("last_project_path", value ?? "");
                ProjectChanged?.Invoke();
            }
        }
    }

    public string? CurrentName =>
        !string.IsNullOrWhiteSpace(CurrentPath) ? Path.GetFileName(CurrentPath) : null;

    public bool HasProject =>
        !string.IsNullOrWhiteSpace(CurrentPath)
        && File.Exists(Path.Combine(CurrentPath!, "artisan"));

    public event Action? ProjectChanged;

    public bool IsLaravelProject(string path) =>
        File.Exists(Path.Combine(path, "artisan"))
        && File.Exists(Path.Combine(path, "composer.json"));
}