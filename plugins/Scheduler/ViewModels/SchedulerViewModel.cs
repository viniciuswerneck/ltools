using System.Collections.ObjectModel;
using Avalonia.Threading;
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
    private string _statusMessage = "Selecione um projeto no menu lateral.";

    public ObservableCollection<ScheduledTask> Tasks { get; } = [];

    public SchedulerViewModel()
    {
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        InitFromContext();
    }

    private void InitFromContext()
    {
        var path = ProjectContext.Instance.CurrentPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            _projectPath = path;
            ProjectName = ProjectContext.Instance.CurrentName ?? "";
            _ = LoadTasksAsync();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Tasks.Clear();
            InitFromContext();
        });
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