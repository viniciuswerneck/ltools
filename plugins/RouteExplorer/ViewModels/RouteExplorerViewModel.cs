using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using Avalonia.Input.Platform;
using Avalonia.Threading;
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
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _middlewareFilter = string.Empty;

    [ObservableProperty]
    private RouteInfo? _selectedRoute;

    [ObservableProperty]
    private bool _hasSelectedRoute;

    [ObservableProperty]
    private bool _hasDetailContent;

    [ObservableProperty]
    private int _totalRoutes;

    private string _routeDetail = string.Empty;
    public string DetailContent
    {
        get => _routeDetail;
        set => SetProperty(ref _routeDetail, value);
    }

    [ObservableProperty]
    private int _filteredCount;

    [ObservableProperty]
    private int _namedRoutes;

    [ObservableProperty]
    private string _methodBreakdown = string.Empty;

    [ObservableProperty]
    private bool _filterGet = true;

    [ObservableProperty]
    private bool _filterPost = true;

    [ObservableProperty]
    private bool _filterPut = true;

    [ObservableProperty]
    private bool _filterPatch = true;

    [ObservableProperty]
    private bool _filterDelete = true;

    [ObservableProperty]
    private bool _filterHead = true;

    [ObservableProperty]
    private bool _filterOptions = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoProject))]
    private bool _hasProject;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoRoutes))]
    private bool _hasRoutes;

    [ObservableProperty]
    private bool _showDuplicates;

    [ObservableProperty]
    private int _duplicateCount;

    [ObservableProperty]
    private int _unnamedCount;

    public bool HasNoProject => !HasProject;

    public bool HasNoRoutes => HasProject && !HasRoutes;

    public ObservableCollection<RouteInfo> AllRoutes { get; } = [];

    public ObservableCollection<RouteInfo> FilteredRoutes { get; } = [];

    public ObservableCollection<RouteGroup> RouteGroups { get; } = [];

    public ObservableCollection<string> AvailableMethods { get; } = ["GET", "HEAD", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];

    [ObservableProperty]
    private int _selectedGroupMode;

    public bool IsFlatView => SelectedGroupMode == 0;
    public bool IsGroupedView => SelectedGroupMode != 0;
    public bool IsDetailVisible => HasSelectedRoute || HasDetailContent;

    public RouteExplorerViewModel()
    {
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        InitFromContext();
    }

    private void InitFromContext()
    {
        var path = ProjectContext.Instance.CurrentPath;
        HasProject = !string.IsNullOrWhiteSpace(path);
        if (HasProject)
        {
            _projectPath = path!;
            ProjectName = ProjectContext.Instance.CurrentName ?? "";
            _ = LoadRoutesAsync();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            AllRoutes.Clear();
            FilteredRoutes.Clear();
            RouteGroups.Clear();
            HasRoutes = false;
            SelectedRoute = null;
            HasSelectedRoute = false;
            HasDetailContent = false;
            DetailContent = "";
            TotalRoutes = 0;
            FilteredCount = 0;
            MethodBreakdown = "";
            InitFromContext();
        });
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnMiddlewareFilterChanged(string value) => ApplyFilters();

    partial void OnSelectedGroupModeChanged(int value)
    {
        ApplyFilters();
        OnPropertyChanged(nameof(IsFlatView));
        OnPropertyChanged(nameof(IsGroupedView));
    }

    partial void OnFilterGetChanged(bool value) => ApplyFilters();
    partial void OnFilterPostChanged(bool value) => ApplyFilters();
    partial void OnFilterPutChanged(bool value) => ApplyFilters();
    partial void OnFilterPatchChanged(bool value) => ApplyFilters();
    partial void OnFilterDeleteChanged(bool value) => ApplyFilters();
    partial void OnFilterHeadChanged(bool value) => ApplyFilters();
    partial void OnFilterOptionsChanged(bool value) => ApplyFilters();

    partial void OnHasSelectedRouteChanged(bool value) => OnPropertyChanged(nameof(IsDetailVisible));

    partial void OnHasDetailContentChanged(bool value) => OnPropertyChanged(nameof(IsDetailVisible));

    partial void OnSelectedRouteChanged(RouteInfo? value)
    {
        HasSelectedRoute = value != null;
        if (value == null)
        {
            if (!HasDetailContent)
                DetailContent = string.Empty;
            return;
        }
        HasDetailContent = false;

        var sb = new StringBuilder();
        sb.AppendLine($"Method:     {value.Method}");
        sb.AppendLine($"URI:        /{value.Uri}");
        sb.AppendLine($"Name:       {(!string.IsNullOrWhiteSpace(value.Name) ? value.Name : "(sem nome)")}");
        sb.AppendLine($"Action:     {value.Action}");
        if (value.HasDomain)
            sb.AppendLine($"Domain:     {value.Domain}");
        sb.AppendLine($"Middleware: {(!string.IsNullOrWhiteSpace(value.Middleware) ? value.Middleware : "(nenhum)")}");
        sb.AppendLine($"Controller:  {value.ControllerName}");
        if (value.ActionMethod != null)
            sb.AppendLine($"Method:     {value.ActionMethod}");
        sb.AppendLine($"Vendor:     {(value.IsVendor ? "Sim" : "Não")}");
        DetailContent = sb.ToString().TrimEnd();
    }

    [RelayCommand]
    private async Task LoadRoutesAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
            return;

        StatusMessage = "Carregando rotas...";
        AllRoutes.Clear();
        FilteredRoutes.Clear();
        RouteGroups.Clear();
        HasRoutes = false;
        TotalRoutes = 0;
        HasDetailContent = false;

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan route:list --json --no-ansi");

            if (string.IsNullOrWhiteSpace(output))
            {
                StatusMessage = "Comando não retornou saída. Verifique se o PHP está acessível.";
                return;
            }

            if (output.Contains("No routes", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Nenhuma rota encontrada.";
                return;
            }

            DetailContent = "";

            var parsed = ParseRoutesJson(output);
            if (parsed == 0)
            {
                var preview = output.Length > 300 ? output[..300] + "..." : output;
                DetailContent = $"RAW OUTPUT:\n{preview}";
                HasDetailContent = true;
                StatusMessage = "Não foi possível interpretar a saída. Veja detalhes abaixo.";
                return;
            }

            ComputeStats();
            ApplyFilters();

            StatusMessage = HasRoutes
                ? $"{TotalRoutes} rota(s) carregada(s) | {NamedRoutes} nomeada(s)"
                : "Nenhuma rota encontrada.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar rotas: {ex.Message}";
        }
    }

    private int ParseRoutesJson(string raw)
    {
        var start = raw.IndexOf('[');
        if (start < 0)
        {
            start = raw.IndexOf('{');
            if (start < 0) return ParseRoutesTable(raw);
            raw = raw[start..];
            var end = raw.LastIndexOf('}');
            if (end > 0) raw = raw[..(end + 1)];
        }
        else
        {
            raw = raw[start..];
            var end = raw.LastIndexOf(']');
            if (end > 0) raw = raw[..(end + 1)];
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var route in doc.RootElement.EnumerateArray())
                    AllRoutes.Add(ParseRouteElement(route));
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var route in doc.RootElement.EnumerateObject())
                {
                    if (route.Value.ValueKind == JsonValueKind.Object)
                        AllRoutes.Add(ParseRouteElement(route.Value));
                }
            }
        }
        catch
        {
            return ParseRoutesTable(raw);
        }

        return AllRoutes.Count;
    }

    private static RouteInfo ParseRouteElement(JsonElement el)
    {
        return new RouteInfo
        {
            Method = TryGetString(el, "method"),
            Uri = TryGetString(el, "uri"),
            Name = TryGetString(el, "name"),
            Action = TryGetString(el, "action"),
            Middleware = TryGetMiddleware(el),
            Domain = TryGetString(el, "domain"),
            IsVendor = TryGetBool(el, "vendor")
        };
    }

    private static string TryGetMiddleware(JsonElement el)
    {
        if (!el.TryGetProperty("middleware", out var prop))
            return "";
        return prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetString() ?? "",
            JsonValueKind.Array => string.Join(",", prop.EnumerateArray().Select(x => x.GetString() ?? "")),
            _ => ""
        };
    }

    private int ParseRoutesTable(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var inTable = false;
        var count = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("+-"))
            {
                inTable = !inTable;
                continue;
            }

            if (!inTable || !trimmed.StartsWith("|") || trimmed.Contains("| Method |") || trimmed.Contains("| Domain |"))
                continue;

            var parts = trimmed.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim()).ToArray();

            if (parts.Length < 4) continue;

            AllRoutes.Add(new RouteInfo
            {
                Method = parts[0],
                Uri = parts[1],
                Name = parts.Length > 3 ? parts[3] : "",
                Action = parts.Length > 4 ? parts[4] : "",
                Middleware = parts.Length > 5 ? parts[5] : "",
                Domain = parts.Length > 2 ? parts[2] : ""
            });
            count++;
        }

        return count;
    }

    private void ComputeStats()
    {
        TotalRoutes = AllRoutes.Count;
        NamedRoutes = AllRoutes.Count(r => r.HasName);
        UnnamedCount = TotalRoutes - NamedRoutes;

        var methodGroups = AllRoutes.SelectMany(r => r.Methods)
            .GroupBy(m => m)
            .Select(g => $"{g.Key}:{g.Count()}");
        MethodBreakdown = string.Join(" ", methodGroups);

        DuplicateCount = AllRoutes.GroupBy(r => r.Uri.ToLowerInvariant())
            .Count(g => g.Count() > 1);

        HasRoutes = TotalRoutes > 0;
    }

    private void ApplyFilters()
    {
        FilteredRoutes.Clear();
        RouteGroups.Clear();

        var query = AllRoutes.AsEnumerable();

        // Method filter
        var activeMethods = new List<string>();
        if (FilterGet) activeMethods.Add("GET");
        if (FilterHead) activeMethods.Add("HEAD");
        if (FilterPost) activeMethods.Add("POST");
        if (FilterPut) activeMethods.Add("PUT");
        if (FilterPatch) activeMethods.Add("PATCH");
        if (FilterDelete) activeMethods.Add("DELETE");
        if (FilterOptions) activeMethods.Add("OPTIONS");
        query = query.Where(r => r.Methods.Any(m => activeMethods.Contains(m)));

        // Search filter
        var search = SearchText?.Trim().ToLowerInvariant() ?? "";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                r.Uri.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Action.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Method.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Middleware filter
        var mw = MiddlewareFilter?.Trim().ToLowerInvariant() ?? "";
        if (!string.IsNullOrWhiteSpace(mw))
            query = query.Where(r => r.Middleware.Contains(mw, StringComparison.OrdinalIgnoreCase));

        // Duplicates only
        if (ShowDuplicates)
        {
            var dupUris = query.GroupBy(r => r.Uri.ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToHashSet();
            query = query.Where(r => dupUris.Contains(r));
        }

        var result = query.ToList();
        FilteredCount = result.Count;

        if (SelectedGroupMode == 0)
        {
            foreach (var r in result)
                FilteredRoutes.Add(r);
        }
        else
        {
            var groups = SelectedGroupMode switch
            {
                1 => result.GroupBy(r => r.GroupPrefix),
                2 => result.GroupBy(r => r.ControllerName),
                3 => result.GroupBy(r => string.IsNullOrWhiteSpace(r.Middleware) ? "(nenhum)" : r.Middleware.Split(',')[0].Trim()),
                _ => result.GroupBy(r => r.Domain)
            };

            foreach (var g in groups.OrderBy(x => x.Key))
            {
                RouteGroups.Add(new RouteGroup
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Routes = new ObservableCollection<RouteInfo>(g)
                });
            }
        }

        if (SelectedRoute != null && !result.Contains(SelectedRoute))
        {
            SelectedRoute = null;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        MiddlewareFilter = string.Empty;
        FilterGet = true;
        FilterPost = true;
        FilterPut = true;
        FilterPatch = true;
        FilterDelete = true;
        FilterHead = true;
        FilterOptions = true;
        ShowDuplicates = false;
        SelectedGroupMode = 0;
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        if (AllRoutes.Count == 0) return;

        var lines = new List<string>
        {
            "Method,URI,Name,Action,Middleware,Domain,Vendor"
        };

        lines.AddRange(AllRoutes.Select(r =>
            $"\"{r.Method}\",\"{r.Uri}\",\"{r.Name}\",\"{r.Action}\",\"{r.Middleware}\",\"{r.Domain}\",\"{(r.IsVendor ? "Yes" : "No")}\""));

        var path = Path.Combine(_projectPath, "routes_export.csv");
        await File.WriteAllLinesAsync(path, lines);
        StatusMessage = $"Rotas exportadas para {path}";
    }

    [RelayCommand]
    private async Task CopySelectedRouteAsync()
    {
        if (SelectedRoute == null) return;

        var text = $"{SelectedRoute.Method} /{SelectedRoute.Uri}";
        if (!string.IsNullOrWhiteSpace(SelectedRoute.Name))
            text += $" [{SelectedRoute.Name}]";
        text += $" → {SelectedRoute.Action}";

        try
        {
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime;
            if (lifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                    StatusMessage = "Rota copiada para área de transferência.";
                    return;
                }
            }
        }
        catch { }
    }

    private static string TryGetString(JsonElement el, string name)
    {
        return el.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
    }

    private static bool TryGetBool(JsonElement el, string name)
    {
        return el.TryGetProperty(name, out var prop) && prop.GetBoolean();
    }
}

public class RouteGroup
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public ObservableCollection<RouteInfo> Routes { get; set; } = [];
}
