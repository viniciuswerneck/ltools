using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Interfaces;
using LTools.Core.Services;

namespace LTools.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IPluginLoader _pluginLoader;

    [ObservableProperty]
    private UserControl? _currentView;

    [ObservableProperty]
    private string _currentPluginName = "Dashboard";

    public ObservableCollection<PluginItemViewModel> Plugins { get; } = [];

    public MainWindowViewModel()
    {
        _pluginLoader = new PluginLoader();
    }

    public async Task InitializeAsync()
    {
        var pluginsPath = GetPluginsPath();
        var plugins = await _pluginLoader.LoadPluginsAsync(pluginsPath);

        foreach (var plugin in plugins)
            Plugins.Add(new PluginItemViewModel(plugin));

        if (Plugins.Count > 0)
            SelectPlugin(Plugins[0]);
    }

    [RelayCommand]
    private void SelectPlugin(PluginItemViewModel? pluginItem)
    {
        if (pluginItem?.Plugin == null)
            return;

        CurrentPluginName = pluginItem.Plugin.Name;
        CurrentView = pluginItem.Plugin.GetView();
    }

    private static string GetPluginsPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var devPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "plugins"));
        if (Directory.Exists(devPath))
            return devPath;

        var releasePath = Path.Combine(baseDir, "plugins");
        Directory.CreateDirectory(releasePath);
        return releasePath;
    }
}

public partial class PluginItemViewModel : ObservableObject
{
    public ILToolsPlugin Plugin { get; }

    public string Name => Plugin.Name;
    public string Icon => Plugin.Icon;

    public PluginItemViewModel(ILToolsPlugin plugin)
    {
        Plugin = plugin;
    }
}
