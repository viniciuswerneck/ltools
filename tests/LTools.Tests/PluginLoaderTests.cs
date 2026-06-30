using LTools.Core.Interfaces;
using LTools.Core.Services;

namespace LTools.Tests;

public class PluginLoaderTests
{
    private static ILogger CreateLogger() => new ConsoleLogger();

    [Fact]
    public void PluginLoader_ShouldStartEmpty()
    {
        var loader = new PluginLoader(CreateLogger());
        Assert.Empty(loader.LoadedPlugins);
    }

    [Fact]
    public void PluginLoader_GetPlugin_ReturnsNull_WhenNotLoaded()
    {
        var loader = new PluginLoader(CreateLogger());
        var result = loader.GetPlugin("NonExistent");
        Assert.Null(result);
    }
}
