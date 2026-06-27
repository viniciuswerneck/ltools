namespace LTools.Core.Interfaces;

public interface IConfigManager
{
    T? Get<T>(string key);
    void Set<T>(string key, T value);
    void Save();
    void Load();
}
