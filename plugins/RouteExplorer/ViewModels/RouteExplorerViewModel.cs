using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.RouteExplorer.Models;

namespace LTools.RouteExplorer.ViewModels;

public partial class RouteExplorerViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _middlewareFilter = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto Laravel para explorar rotas.";

    public ObservableCollection<RouteInfo> AllRoutes { get; } = [];
    public ObservableCollection<RouteInfo> FilteredRoutes { get; } = [];

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
        await LoadRoutesAsync();
    }

    private async Task LoadRoutesAsync()
    {
        StatusMessage = "Carregando rotas...";
        AllRoutes.Clear();
        FilteredRoutes.Clear();

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan route:list --json --no-ansi");
            ParseRoutes(output);
            StatusMessage = $"{AllRoutes.Count} rotas carregadas.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }
    }

    private void ParseRoutes(string json)
    {
        try
        {
            var start = json.IndexOf('[');
            if (start < 0) return;
            json = json[start..];

            using var doc = JsonDocument.Parse(json);
            foreach (var route in doc.RootElement.EnumerateArray())
            {
                var methods = TryGetProperty(route, "method");
                var uri = TryGetProperty(route, "uri");
                var name = TryGetProperty(route, "name");
                var action = TryGetProperty(route, "action");
                var middleware = TryGetProperty(route, "middleware");

                AllRoutes.Add(new RouteInfo
                {
                    Method = methods,
                    Uri = uri,
                    Name = name,
                    Action = action,
                    Middleware = middleware
                });
            }

            ApplyFilter();
        }
        catch
        {
            StatusMessage = "Erro ao interpretar lista de rotas.";
        }
    }

    private static string TryGetProperty(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnMiddlewareFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredRoutes.Clear();
        var search = SearchText?.Trim().ToLower() ?? "";
        var middleware = MiddlewareFilter?.Trim().ToLower() ?? "";

        var items = AllRoutes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
            items = items.Where(r =>
                r.Uri.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Action.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(middleware))
            items = items.Where(r =>
                r.Middleware.Contains(middleware, StringComparison.OrdinalIgnoreCase));

        foreach (var r in items)
            FilteredRoutes.Add(r);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = "";
        MiddlewareFilter = "";
    }

    [RelayCommand]
    private async Task ExportRoutesAsync()
    {
        if (AllRoutes.Count == 0) return;

        var lines = new List<string> { "Method|URI|Name|Action|Middleware" };
        lines.AddRange(AllRoutes.Select(r => $"{r.Method}|{r.Uri}|{r.Name}|{r.Action}|{r.Middleware}"));

        var path = Path.Combine(_projectPath, "routes_export.csv");
        await File.WriteAllLinesAsync(path, lines);
        StatusMessage = $"Rotas exportadas para {path}";
    }
}