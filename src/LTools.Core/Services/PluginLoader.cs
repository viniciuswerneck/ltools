using System.Reflection;
using System.Runtime.Loader;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class PluginLoader : IPluginLoader
{
    private static string? _pluginsResolvePath;
    private readonly ILogger _logger;

    public List<ILToolsPlugin> LoadedPlugins { get; } = [];

    public PluginLoader(ILogger logger)
    {
        _logger = logger;
    }

    static PluginLoader()
    {
        AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
        {
            if (_pluginsResolvePath == null)
                return null;

            var name = assemblyName.Name;
            if (name == null) return null;

            var path = Path.Combine(_pluginsResolvePath, $"{name}.dll");
            return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
        };
    }

    public Task<List<ILToolsPlugin>> LoadPluginsAsync(string pluginsPath)
    {
        _pluginsResolvePath = pluginsPath;
        LoadedPlugins.Clear();
        var plugins = new List<ILToolsPlugin>();

        if (!Directory.Exists(pluginsPath))
            return Task.FromResult(plugins);

        var dllFiles = Directory.GetFiles(pluginsPath, "LTools.*.dll");

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

                _logger.Debug($"Plugin carregado: {Path.GetFileName(dll)}");
            }
            catch (Exception ex)
            {
                _logger.Warning($"Falha ao carregar DLL: {Path.GetFileName(dll)}", ex);
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