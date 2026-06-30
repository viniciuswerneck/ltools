using System.Security.Cryptography;
using System.Text;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ProjectProfileService : IProjectProfileService
{
    private readonly IConfigManager _config;
    private string? _projectHash;
    private const string LastPluginKey = "profile_last_plugin";
    private const string LastEnvFileKey = "profile_last_env";
    private const string LastDatabaseConnectionKey = "profile_last_db";

    public ProjectProfileService(IConfigManager config)
    {
        _config = config;
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        UpdateHash();
    }

    private void OnProjectChanged() => UpdateHash();

    private void UpdateHash()
    {
        var path = ProjectContext.Instance.CurrentPath;
        _projectHash = !string.IsNullOrWhiteSpace(path)
            ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path)))[..12]
            : null;
    }

    private string Scoped(string key) => _projectHash != null ? $"{_projectHash}_{key}" : key;

    public T? Get<T>(string key) => _config.Get<T>(Scoped(key));

    public void Set<T>(string key, T value) => _config.Set(Scoped(key), value);

    public string? LastPlugin
    {
        get => Get<string>(LastPluginKey);
        set => Set(LastPluginKey, value);
    }

    public string? LastEnvFile
    {
        get => Get<string>(LastEnvFileKey);
        set => Set(LastEnvFileKey, value);
    }

    public string? LastDatabaseConnection
    {
        get => Get<string>(LastDatabaseConnectionKey);
        set => Set(LastDatabaseConnectionKey, value);
    }
}
