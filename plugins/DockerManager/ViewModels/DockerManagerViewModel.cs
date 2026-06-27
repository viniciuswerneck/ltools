using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;

namespace LTools.DockerManager.ViewModels;

public partial class DockerManagerViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _hasDockerCompose;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto Laravel com Docker Compose.";

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
        HasDockerCompose = File.Exists(Path.Combine(_projectPath, "docker-compose.yml"))
            || File.Exists(Path.Combine(_projectPath, "docker-compose.yaml"));

        if (HasDockerCompose)
            StatusMessage = "Docker Compose detectado!";
        else
            StatusMessage = "Nenhum docker-compose.yml encontrado no projeto.";
    }

    private async Task RunDockerAsync(string arguments)
    {
        if (string.IsNullOrWhiteSpace(_projectPath)) return;

        IsRunning = true;
        OutputText = "";
        StatusMessage = $"Executando docker-compose {arguments}...";

        try
        {
            _runner.OutputReceived += OnOutputReceived;
            _runner.ErrorReceived += OnErrorReceived;

            await _runner.RunAsync(_projectPath, "docker-compose", arguments);

            StatusMessage = "Comando concluído.";
        }
        catch (Exception ex)
        {
            OutputText += $"\nErro: {ex.Message}";
        }
        finally
        {
            _runner.OutputReceived -= OnOutputReceived;
            _runner.ErrorReceived -= OnErrorReceived;
            IsRunning = false;
        }
    }

    private void OnOutputReceived(string data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => OutputText += data + "\n");
    }

    private void OnErrorReceived(string data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => OutputText += $"[ERRO] {data}\n");
    }

    [RelayCommand] private async Task UpAsync() => await RunDockerAsync("up -d");
    [RelayCommand] private async Task StopAsync() => await RunDockerAsync("stop");
    [RelayCommand] private async Task DownAsync() => await RunDockerAsync("down");
    [RelayCommand] private async Task RebuildAsync() => await RunDockerAsync("up -d --build");
    [RelayCommand] private async Task LogsAsync() => await RunDockerAsync("logs --tail=100");
    [RelayCommand] private async Task PsAsync() => await RunDockerAsync("ps");
    [RelayCommand] private void ClearOutput() => OutputText = "";
}