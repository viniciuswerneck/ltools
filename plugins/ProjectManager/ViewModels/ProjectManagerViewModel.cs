using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Models;
using LTools.Core.Services;

namespace LTools.ProjectManager.ViewModels;

public partial class ProjectManagerViewModel : ObservableObject
{
    private readonly LaravelDetector _detector = new();

    [ObservableProperty]
    private string _searchPath = string.Empty;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusMessage = "Selecione uma pasta para escanear projetos Laravel.";

    public ObservableCollection<ProjectItemViewModel> Projects { get; } = [];

    [RelayCommand]
    private async Task SelectFolderAsync()
    {
        var window = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window?.StorageProvider == null) return;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Selecione a pasta para escanear",
            AllowMultiple = false
        });

        var folder = folders?.FirstOrDefault();
        if (folder == null) return;

        SearchPath = folder.Path.LocalPath;
        await ScanAsync();
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchPath) || !Directory.Exists(SearchPath))
        {
            StatusMessage = "Caminho inválido. Selecione uma pasta válida.";
            return;
        }

        IsScanning = true;
        StatusMessage = "Escaneando...";
        Projects.Clear();

        try
        {
            var projects = await _detector.ScanAsync(SearchPath);

            foreach (var project in projects)
                Projects.Add(new ProjectItemViewModel(project));

            StatusMessage = projects.Count > 0
                ? $"{projects.Count} projeto(s) Laravel encontrado(s)."
                : "Nenhum projeto Laravel encontrado nesta pasta.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao escanear: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void OpenProject(ProjectItemViewModel? item)
    {
        if (item?.Project == null) return;

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = item.Project.Path,
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
    }
}

public partial class ProjectItemViewModel : ObservableObject
{
    public LaravelProject Project { get; }

    public string Name => Project.Name;
    public string LaravelVersion => Project.LaravelVersion;
    public string PhpVersion => Project.PhpVersion;
    public string Database => string.IsNullOrWhiteSpace(Project.Database) ? "N/D" : Project.Database;
    public string Environment => string.IsNullOrWhiteSpace(Project.Environment) ? "N/D" : Project.Environment;
    public string FormattedSize => FormatSize(Project.SizeInBytes);

    public ProjectItemViewModel(LaravelProject project)
    {
        Project = project;
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }
}
