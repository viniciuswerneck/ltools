using System.Reflection;
using System.Runtime.Loader;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class PluginLoader : IPluginLoader
{
    public List<ILToolsPlugin> LoadedPlugins { get; } = [];

    public Task<List<ILToolsPlugin>> LoadPluginsAsync(string pluginsPath)
    {
        LoadedPlugins.Clear();
        var plugins = new List<ILToolsPlugin>();

        if (!Directory.Exists(pluginsPath))
            return Task.FromResult(plugins);

        var dllFiles = Directory.GetFiles(pluginsPath, "*.dll");

        foreach (var dll in dllFiles)
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dll));
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(ILToolsPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    if (Activator.CreateInstance(type) is ILToolsPlugin plugin)
                        plugins.Add(plugin);
                }
            }
            catch
            {
                // Skip DLLs that fail to load
            }
        }

        LoadedPlugins.AddRange(plugins);
        return Task.FromResult(plugins);
    }

    public ILToolsPlugin? GetPlugin(string name)
    {
        return LoadedPlugins.FirstOrDefault(p => p.Name == name);
    }
}
