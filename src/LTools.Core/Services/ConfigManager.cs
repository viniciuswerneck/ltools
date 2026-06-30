using System.Text.Json;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ConfigManager : IConfigManager
{
    private readonly string _configPath;
    private Dictionary<string, object> _config = [];
    private readonly Lock _lock = new();
    private CancellationTokenSource? _debounceCts;

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
        lock (_lock)
        {
            if (_config.TryGetValue(key, out var value))
            {
                if (value is JsonElement element)
                    return JsonSerializer.Deserialize<T>(element.GetRawText());
                if (value is T typed)
                    return typed;
            }
            return default;
        }
    }

    public void Set<T>(string key, T value)
    {
        var serialized = JsonSerializer.SerializeToElement(value);
        lock (_lock)
        {
            _config[key] = serialized;
        }
        DebouncedSave();
    }

    private void DebouncedSave()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
                Save();
            }
            catch (OperationCanceledException) { }
        }, token);
    }

    public void Save()
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
    }

    public void Load()
    {
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            lock (_lock)
            {
                _config = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? [];
            }
        }
    }
}
