using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.ComposerManager.Models;
using LTools.Core.Services;

namespace LTools.ComposerManager.ViewModels;

public partial class ComposerManagerViewModel : ObservableObject
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
    private string _statusMessage = "Selecione um projeto Laravel para gerenciar dependências Composer.";

    [ObservableProperty]
    private string _packageName = string.Empty;

    public ObservableCollection<ComposerPackage> Packages { get; } = [];
    public ObservableCollection<ComposerHistory> History { get; } = [];

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

        if (!IsLaravelProject(_projectPath))
        {
            StatusMessage = "A pasta selecionada não contém um projeto Laravel.";
            return;
        }

        ProjectName = Path.GetFileName(_projectPath);
        await LoadPackagesAsync();
    }

    private static bool IsLaravelProject(string path)
    {
        return File.Exists(Path.Combine(path, "artisan"))
            && File.Exists(Path.Combine(path, "composer.json"));
    }

    private async Task LoadPackagesAsync()
    {
        Packages.Clear();
        StatusMessage = "Carregando dependências...";

        try
        {
            var json = await File.ReadAllTextAsync(Path.Combine(_projectPath, "composer.json"));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("require", out var require))
                AddPackages(require, "require");

            if (root.TryGetProperty("require-dev", out var requireDev))
                AddPackages(requireDev, "require-dev");

            StatusMessage = $"{Packages.Count} dependências encontradas.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao ler composer.json: {ex.Message}";
        }
    }

    private void AddPackages(JsonElement element, string type)
    {
        foreach (var pkg in element.EnumerateObject())
        {
            Packages.Add(new ComposerPackage
            {
                Name = pkg.Name,
                Version = pkg.Value.GetString() ?? "",
                Type = type
            });
        }
    }

    private async Task RunComposerAsync(string arguments)
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        IsRunning = true;
        OutputText = "";
        StatusMessage = $"Executando composer {arguments}...";

        try
        {
            _runner.OutputReceived += OnOutputReceived;
            _runner.ErrorReceived += OnErrorReceived;

            await _runner.RunAsync(_projectPath, "composer", arguments);

            var cmdParts = arguments.Split(' ');
            History.Insert(0, new ComposerHistory
            {
                Command = cmdParts[0],
                Arguments = arguments,
                ExecutedAt = DateTime.Now
            });

            StatusMessage = "Comando concluído.";
            await LoadPackagesAsync();
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

    [RelayCommand] private async Task InstallAsync() => await RunComposerAsync("install");
    [RelayCommand] private async Task UpdateAsync() => await RunComposerAsync("update");
    [RelayCommand] private async Task RequireAsync()
    {
        if (!string.IsNullOrWhiteSpace(PackageName))
            await RunComposerAsync($"require {PackageName}");
    }
    [RelayCommand] private async Task RemoveAsync()
    {
        if (!string.IsNullOrWhiteSpace(PackageName))
            await RunComposerAsync($"remove {PackageName}");
    }
    [RelayCommand] private async Task OutdatedAsync() => await RunComposerAsync("outdated");
    [RelayCommand] private async Task ShowAsync() => await RunComposerAsync("show --latest");
    [RelayCommand] private void ClearOutput() => OutputText = "";
}