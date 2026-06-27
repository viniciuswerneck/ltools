using LTools.Core.Models;

namespace LTools.Core.Interfaces;

public interface IPluginLoader
{
    List<ILToolsPlugin> LoadedPlugins { get; }
    Task<List<ILToolsPlugin>> LoadPluginsAsync(string pluginsPath);
    ILToolsPlugin? GetPlugin(string name);
}
