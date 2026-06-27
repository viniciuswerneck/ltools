using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;

namespace LTools.QueueMonitor.ViewModels;

public partial class QueueMonitorViewModel : ObservableObject
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
    private string _statusMessage = "Selecione um projeto na barra superior.";

    public QueueMonitorViewModel()
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
            StatusMessage = "Projeto selecionado. Escolha uma ação de fila.";
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            OutputText = string.Empty;
            InitFromContext();
        });
    }

    private async Task RunArtisanAsync(string arguments)
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        IsRunning = true;
        OutputText = "";
        StatusMessage = $"Executando php artisan {arguments}...";

        try
        {
            _runner.OutputReceived += OnOutputReceived;
            _runner.ErrorReceived += OnErrorReceived;

            await _runner.RunAsync(_projectPath, "php", $"artisan {arguments}");

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

    [RelayCommand] private async Task CheckFailedAsync() => await RunArtisanAsync("queue:failed");
    [RelayCommand] private async Task FlushFailedAsync() => await RunArtisanAsync("queue:flush");
    [RelayCommand] private async Task RetryFailedAsync() => await RunArtisanAsync("queue:retry all");
    [RelayCommand] private async Task WorkAsync() => await RunArtisanAsync("queue:work --once");
    [RelayCommand] private async Task RestartAsync() => await RunArtisanAsync("queue:restart");
    [RelayCommand] private async Task TableAsync() => await RunArtisanAsync("queue:table");
    [RelayCommand] private async Task MonitorAsync() => await RunArtisanAsync("queue:monitor");
    [RelayCommand] private void ClearOutput() => OutputText = "";
}