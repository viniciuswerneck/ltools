using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LTools.Core.Interfaces;
using LTools.Core.Services;
using LTools.UI.Services;
using LTools.UI.ViewModels;
using LTools.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LTools.UI;

public partial class App : Application
{
    public static ThemeService ThemeService { get; } = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfigManager, ConfigManager>();
        services.AddSingleton<ILogger, ConsoleLogger>();
        services.AddSingleton<IMessenger, Messenger>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IToolVersionService, ToolVersionService>();
        services.AddSingleton<IProjectProfileService, ProjectProfileService>();
        services.AddSingleton<IWatcherService, WatcherService>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddTransient<IProcessRunner, ProcessRunner>();
        services.AddTransient<ILaravelDetector, LaravelDetector>();

        var provider = services.BuildServiceProvider();

        AppServices.Initialize(provider);

        ThemeService.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splash = new SplashWindow();
            desktop.MainWindow = splash;
            splash.ShowAndThenOpenMain();
        }

        base.OnFrameworkInitializationCompleted();
    }
}