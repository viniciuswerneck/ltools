using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGlobalProject))]
    private string? _globalProjectName;

    [ObservableProperty]
    private string? _globalProjectPath;

    public bool HasGlobalProject => GlobalProjectName != null;

    public ObservableCollection<PluginItemViewModel> Plugins { get; } = [];

    public string AppVersion { get; } = GetVersion();

    private static string GetVersion()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
    }

    [RelayCommand]
    private void OpenSite()
    {
        try { Process.Start(new ProcessStartInfo("https://lab.werneck.dev.br/") { UseShellExecute = true }); }
        catch { }
    }

    public MainWindowViewModel()
    {
        _pluginLoader = new PluginLoader();
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        UpdateProjectName();
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

    private void OnProjectChanged()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(UpdateProjectName);
    }

    private void UpdateProjectName()
    {
        GlobalProjectName = ProjectContext.Instance.CurrentName;
        GlobalProjectPath = ProjectContext.Instance.CurrentPath;
    }

    [RelayCommand]
    private async Task SelectGlobalProjectAsync()
    {
        var window = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window?.StorageProvider == null) return;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Selecione um projeto Laravel",
            AllowMultiple = false
        });

        var folder = folders?.FirstOrDefault();
        if (folder == null) return;

        var path = folder.Path.LocalPath;

        if (!ProjectContext.IsLaravelProject(path))
        {
            return;
        }

        ProjectContext.Instance.CurrentPath = path;
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