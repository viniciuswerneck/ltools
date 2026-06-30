using LTools.Core.Models;

namespace LTools.Core.Interfaces;

/// <summary>Discovers and loads plugins from disk using reflection.</summary>
public interface IPluginLoader
{
    /// <summary>Currently loaded plugin instances.</summary>
    List<ILToolsPlugin> LoadedPlugins { get; }
    /// <summary>Loads all valid plugins from the specified directory.</summary>
    Task<List<ILToolsPlugin>> LoadPluginsAsync(string pluginsPath);
    /// <summary>Finds a loaded plugin by display name (case-insensitive).</summary>
    ILToolsPlugin? GetPlugin(string name);
}
