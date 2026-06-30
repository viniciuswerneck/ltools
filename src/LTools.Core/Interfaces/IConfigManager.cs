namespace LTools.Core.Interfaces;

/// <summary>Persistent configuration storage (JSON-based).</summary>
public interface IConfigManager
{
    /// <summary>Retrieves a configuration value by key.</summary>
    T? Get<T>(string key);
    /// <summary>Sets a configuration value by key.</summary>
    void Set<T>(string key, T value);
    /// <summary>Persists all configuration to disk.</summary>
    void Save();
    /// <summary>Loads configuration from disk.</summary>
    void Load();
}
