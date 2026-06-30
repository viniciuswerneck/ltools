using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ProjectContext
{
    private static readonly Lazy<ProjectContext> _instance = new(() => new ProjectContext());
    public static ProjectContext Instance => _instance.Value;

    private static IConfigManager? _configuredConfig;

    private readonly IConfigManager _config;
    private string? _currentPath;

    public static void Configure(IConfigManager config)
    {
        _configuredConfig = config;
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

    public string? CurrentName => !string.IsNullOrWhiteSpace(CurrentPath)
        ? Path.GetFileName(CurrentPath) : null;

    public bool HasProject => !string.IsNullOrWhiteSpace(CurrentPath)
        && File.Exists(Path.Combine(CurrentPath!, "artisan"));

    public event Action? ProjectChanged;

    private ProjectContext()
    {
        _config = _configuredConfig ?? new ConfigManager();
        var lastPath = _config.Get<string>("last_project_path");
        if (!string.IsNullOrWhiteSpace(lastPath) && Directory.Exists(lastPath))
            _currentPath = lastPath;
    }

    public static bool IsLaravelProject(string path)
    {
        return File.Exists(Path.Combine(path, "artisan"))
            && File.Exists(Path.Combine(path, "composer.json"));
    }
}