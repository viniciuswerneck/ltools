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
    private readonly ILogger _logger;
    private readonly IProjectProfileService? _profile;

    [ObservableProperty]
    private bool _isPaneOpen = true;

    [ObservableProperty]
    private UserControl? _currentView;

    [ObservableProperty]
    private string _currentPluginName = "Dashboard";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGlobalProject))]
    private string? _globalProjectName;

    [ObservableProperty]
    private string? _globalProjectPath;

    [ObservableProperty]
    private string? _statusMessage;

    public bool HasGlobalProject => GlobalProjectName != null;

    public ObservableCollection<PluginItemViewModel> Plugins { get; } = [];

    public string AppVersion { get; } = GetVersion();
    public string ThemeIcon => App.ThemeService.CurrentTheme == ThemeVariant.Light ? "☀️" : "🌙";
public string ThemeLabel => App.ThemeService.CurrentTheme == ThemeVariant.Light ? "Claro" : "Escuro";

    public MainWindowViewModel(IPluginLoader pluginLoader, ILogger logger)
    {
        _pluginLoader = pluginLoader;
        _logger = logger;
        _profile = AppServices.Get<IProjectProfileService>();
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        UpdateProjectName();
    }

    private static string GetVersion()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
    }

    [RelayCommand]
    private void OpenSite()
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://lab.werneck.dev.br/") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logger.Warning("Falha ao abrir site", ex);
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        App.ThemeService.Toggle();
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeLabel));
    }

    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [RelayCommand]
    private void NextPlugin()
    {
        if (Plugins.Count == 0) return;
        var current = Plugins.Select((p, i) => (p, i)).FirstOrDefault(x => x.p.IsSelected);
        var nextIndex = (current.i + 1) % Plugins.Count;
        SelectPlugin(Plugins[nextIndex]);
    }

    [RelayCommand]
    private void PreviousPlugin()
    {
        if (Plugins.Count == 0) return;
        var current = Plugins.Select((p, i) => (p, i)).FirstOrDefault(x => x.p.IsSelected);
        var prevIndex = (current.i - 1 + Plugins.Count) % Plugins.Count;
        SelectPlugin(Plugins[prevIndex]);
    }

    public async Task InitializeAsync()
    {
        var pluginsPath = GetPluginsPath();
        var plugins = await _pluginLoader.LoadPluginsAsync(pluginsPath);

        foreach (var plugin in plugins)
            Plugins.Add(new PluginItemViewModel(plugin));

        if (Plugins.Count > 0)
        {
            var lastPlugin = _profile?.Get<string>("last_plugin");
            var target = lastPlugin != null
                ? Plugins.FirstOrDefault(p => p.Name.Equals(lastPlugin, StringComparison.OrdinalIgnoreCase))
                : null;
            SelectPlugin(target ?? Plugins[0]);
        }
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
        try
        {
            StatusMessage = null;

            var window = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (window?.StorageProvider == null)
            {
                StatusMessage = "Erro ao acessar o seletor de pastas.";
                return;
            }

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
                StatusMessage = "A pasta selecionada não é um projeto Laravel válido.";
                return;
            }

            ProjectContext.Instance.CurrentPath = path;
            StatusMessage = $"Projeto carregado: {ProjectContext.Instance.CurrentName}";
        }
        catch (Exception ex)
        {
            _logger.Error("Erro ao abrir projeto", ex);
            StatusMessage = $"Erro ao abrir projeto: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SelectPlugin(PluginItemViewModel? pluginItem)
    {
        if (pluginItem?.Plugin == null)
            return;

        foreach (var p in Plugins)
            p.IsSelected = false;

        pluginItem.IsSelected = true;
        CurrentPluginName = pluginItem.Plugin.Name;
        CurrentView = pluginItem.Plugin.GetView();
        _profile?.Set("last_plugin", pluginItem.Plugin.Name);
    }

    private static string GetPluginsPath()
    {
        var exeDir = Path.GetDirectoryName(Environment.ProcessPath)!;
        var devPath = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "..", "..", "plugins"));
        if (Directory.Exists(devPath))
            return devPath;

        var releasePath = Path.Combine(exeDir, "plugins");
        Directory.CreateDirectory(releasePath);
        return releasePath;
    }
}

public partial class PluginItemViewModel : ObservableObject
{
    public ILToolsPlugin Plugin { get; }

    public string Name => Plugin.Name;
    public string Icon => Plugin.Icon;

    [ObservableProperty]
    private bool _isSelected;

    public PluginItemViewModel(ILToolsPlugin plugin)
    {
        Plugin = plugin;
    }
}