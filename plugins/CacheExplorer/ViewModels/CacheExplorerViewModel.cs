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
    private string _statusMessage = "Selecione um projeto Laravel para gerenciar caches.";

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
        StatusMessage = "Projeto selecionado. Escolha uma ação de cache.";
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

    [RelayCommand] private async Task ClearApplicationAsync() => await RunArtisanAsync("cache:clear");
    [RelayCommand] private async Task ClearConfigAsync() => await RunArtisanAsync("config:clear");
    [RelayCommand] private async Task ClearRouteAsync() => await RunArtisanAsync("route:clear");
    [RelayCommand] private async Task ClearViewAsync() => await RunArtisanAsync("view:clear");
    [RelayCommand] private async Task ClearEventAsync() => await RunArtisanAsync("event:clear");
    [RelayCommand] private async Task ClearAllAsync()
    {
        await RunArtisanAsync("cache:clear");
        await RunArtisanAsync("config:clear");
        await RunArtisanAsync("route:clear");
        await RunArtisanAsync("view:clear");
        await RunArtisanAsync("event:clear");
        StatusMessage = "Todos os caches foram limpos!";
    }
    [RelayCommand] private void ClearOutput() => OutputText = "";
}