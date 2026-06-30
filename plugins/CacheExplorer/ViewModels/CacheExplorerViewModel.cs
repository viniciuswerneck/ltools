using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;

namespace LTools.CacheExplorer.ViewModels;

public partial class CacheExplorerViewModel : ObservableObject
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

    public CacheExplorerViewModel()
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
            StatusMessage = "Projeto selecionado. Escolha uma ação de cache.";
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
        StatusMessage = $"Executando php artisan {arguments}...";

        OutputText += $"> php artisan {arguments}\n\n";

        try
        {
            _runner.OutputReceived += OnOutputReceived;
            _runner.ErrorReceived += OnErrorReceived;

            await _runner.RunAsync(_projectPath, "php", $"artisan {arguments} --no-ansi");

            OutputText += "\n";
            StatusMessage = "Comando concluído.";
        }
        catch (Exception ex)
        {
            OutputText += $"\nErro: {ex.Message}\n";
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
        Avalonia.Threading.Dispatcher.UIThread.Post(() => OutputText += data.StripAnsi() + "\n");
    }

    private void OnErrorReceived(string data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => OutputText += $"[ERRO] {data.StripAnsi()}\n");
    }

    [RelayCommand] private async Task ClearAllAsync()
    {
        await RunArtisanAsync("cache:clear");
        await RunArtisanAsync("config:clear");
        await RunArtisanAsync("route:clear");
        await RunArtisanAsync("view:clear");
        await RunArtisanAsync("event:clear");
        StatusMessage = "Todos os caches foram limpos!";
    }
    [RelayCommand] private async Task ClearApplicationAsync() => await RunArtisanAsync("cache:clear");
    [RelayCommand] private async Task ClearConfigAsync() => await RunArtisanAsync("config:clear");
    [RelayCommand] private async Task ClearRouteAsync() => await RunArtisanAsync("route:clear");
    [RelayCommand] private async Task ClearViewAsync() => await RunArtisanAsync("view:clear");
    [RelayCommand] private async Task ClearEventAsync() => await RunArtisanAsync("event:clear");
    [RelayCommand] private void ClearOutput() => OutputText = "";
}