using System.Text.Json;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ConfigManager : IConfigManager
{
    private readonly string _configPath;
    private Dictionary<string, object> _config = [];

    public ConfigManager()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "LTools");
        Directory.CreateDirectory(appFolder);
        _configPath = Path.Combine(appFolder, "config.json");
        Load();
    }

    public T? Get<T>(string key)
    {
        if (_config.TryGetValue(key, out var value) && value is JsonElement element)
            return JsonSerializer.Deserialize<T>(element.GetRawText());
        return default;
    }

    public void Set<T>(string key, T value)
    {
        _config[key] = value!;
        Save();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }

    public void Load()
    {
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            _config = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? [];
        }
    }
}
