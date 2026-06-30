using LTools.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LTools.Core.Services;

public static class AppServices
{
    private static IServiceProvider? _provider;

    public static void Initialize(IServiceProvider provider)
    {
        _provider = provider;
        var config = provider.GetService<IConfigManager>();
        if (config != null)
            ProjectContext.Configure(config);
    }

    public static T? Get<T>() where T : class =>
        _provider?.GetService<T>();

    public static T GetRequired<T>() where T : class =>
        _provider?.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
}