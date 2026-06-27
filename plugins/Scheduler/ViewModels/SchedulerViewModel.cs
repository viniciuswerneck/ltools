using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.Scheduler.Models;

namespace LTools.Scheduler.ViewModels;

public partial class SchedulerViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto Laravel para ver tarefas agendadas.";

    public ObservableCollection<ScheduledTask> Tasks { get; } = [];

    [RelayCommand]
    private async Task SelectProjectAsync()
    {
        var window = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window?.StorageProvider == null) return;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Selecione um projeto Laravel",
            AllowMultiple = false
        });

        var folder = folders?.FirstOrDefault();
        if (folder == null) return;

        _projectPath = folder.Path.LocalPath;

        if (!File.Exists(Path.Combine(_projectPath, "artisan")))
        {
            StatusMessage = "A pasta selecionada não contém um projeto Laravel.";
            return;
        }

        ProjectName = Path.GetFileName(_projectPath);
        await LoadTasksAsync();
    }

    private async Task LoadTasksAsync()
    {
        StatusMessage = "Carregando tarefas agendadas...";
        Tasks.Clear();

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan schedule:list --no-ansi");
            ParseTasks(output);
            StatusMessage = $"{Tasks.Count} tarefas agendadas encontradas.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }
    }

    private void ParseTasks(string output)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var started = false;

        foreach (var line in lines)
        {
            if (line.Contains("----")) { started = true; continue; }
            if (!started) continue;

            var parts = line.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length >= 3)
            {
                Tasks.Add(new ScheduledTask
                {
                    Command = parts.Length > 1 ? parts[1].Trim() : "",
                    Schedule = parts.Length > 0 ? parts[0].Trim() : "",
                    Description = parts.Length > 2 ? parts[2].Trim() : "",
                    NextDue = parts.Length > 3 ? parts[3].Trim() : "",
                    Timezone = parts.Length > 4 ? parts[4].Trim() : ""
                });
            }
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadTasksAsync();
}