using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.SqlDebugger.Models;
using LTools.SqlDebugger.Services;

namespace LTools.SqlDebugger.ViewModels;

public partial class SqlDebuggerViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;
    private MySqlQueryWatcher? _watcher;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _dbConnection = string.Empty;

    [ObservableProperty]
    private string _dbHost = string.Empty;

    [ObservableProperty]
    private string _dbPort = string.Empty;

    [ObservableProperty]
    private string _dbName = string.Empty;

    [ObservableProperty]
    private string _dbUser = string.Empty;

    [ObservableProperty]
    private string _dbPass = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isMonitoring;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private int _totalQueries;

    [ObservableProperty]
    private bool _showSlowOnly;

    [ObservableProperty]
    private int _filteredCount;

    public bool HasNoDb => !IsConnected;
    public bool CanMonitor => IsConnected && !IsMonitoring;
    public bool CanStop => IsMonitoring;
    public bool HasQueries => TotalQueries > 0;

    public ObservableCollection<SqlQuery> AllQueries { get; } = [];
    public ObservableCollection<SqlQuery> FilteredQueries { get; } = [];

    public SqlDebuggerViewModel()
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
            _ = LoadDbConfigAsync();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await StopMonitoringAsync();
            AllQueries.Clear();
            FilteredQueries.Clear();
            IsConnected = false;
            IsMonitoring = false;
            DbConnection = string.Empty;
            DbHost = string.Empty;
            DbPort = string.Empty;
            DbName = string.Empty;
            DbUser = string.Empty;
            DbPass = string.Empty;
            TotalQueries = 0;
            FilteredCount = 0;
            SearchText = string.Empty;
            ShowSlowOnly = false;
            InitFromContext();
        });
    }

    private async Task LoadDbConfigAsync()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath))
        {
            StatusMessage = ".env não encontrado.";
            return;
        }

        var env = await File.ReadAllTextAsync(envPath);
        var lines = env.Split('\n');
        string? conn = null, host = null, port = null, name = null, user = null, pass = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("DB_CONNECTION=")) conn = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_HOST=")) host = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_PORT=")) port = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_DATABASE=")) name = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_USERNAME=")) user = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_PASSWORD=")) pass = trimmed.Split('=', 2)[1];
        }

        DbConnection = conn ?? "mysql";
        DbHost = host ?? "127.0.0.1";
        DbPort = port ?? "3306";
        DbName = name ?? "";
        DbUser = user ?? "root";
        DbPass = pass ?? "";

        if (string.IsNullOrWhiteSpace(DbHost) || string.IsNullOrWhiteSpace(DbName))
        {
            StatusMessage = "Banco não configurado no .env.";
            return;
        }

        if (DbConnection != "mysql")
        {
            StatusMessage = $"SQL Monitor requer MySQL (detectado: {DbConnection}).";
            return;
        }

        IsConnected = true;
        StatusMessage = $"Banco: {DbName} em {DbHost}:{DbPort}. Pronto para monitorar.";
        OnPropertyChanged(nameof(HasNoDb));
        OnPropertyChanged(nameof(CanMonitor));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnShowSlowOnlyChanged(bool value) => ApplyFilter();
    partial void OnTotalQueriesChanged(int value) => OnPropertyChanged(nameof(HasQueries));

    private void ApplyFilter()
    {
        var query = AllQueries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            query = query.Where(q => q.Sql.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        if (ShowSlowOnly)
            query = query.Where(q => q.IsSlow);

        FilteredQueries.Clear();
        foreach (var q in query)
            FilteredQueries.Add(q);
        FilteredCount = FilteredQueries.Count;
    }

    [RelayCommand]
    private async Task StartMonitoringAsync()
    {
        if (!IsConnected || string.IsNullOrWhiteSpace(DbHost) || string.IsNullOrWhiteSpace(DbName))
        {
            StatusMessage = "Configuração de banco incompleta. Verifique o .env.";
            return;
        }

        if (!int.TryParse(DbPort, out var port))
            port = 3306;

        AllQueries.Clear();
        FilteredQueries.Clear();
        TotalQueries = 0;
        FilteredCount = 0;

        _watcher = new MySqlQueryWatcher(DbHost, port, DbName, DbUser, DbPass);
        _watcher.QueryReceived += OnQueryReceived;
        _watcher.StatusChanged += msg => Dispatcher.UIThread.Post(() => StatusMessage = msg);
        _watcher.ErrorOccurred += err =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                StatusMessage = err;
                IsMonitoring = false;
                OnPropertyChanged(nameof(CanMonitor));
                OnPropertyChanged(nameof(CanStop));
            });
        };

        await _watcher.StartAsync();
        IsMonitoring = _watcher.IsRunning;
        OnPropertyChanged(nameof(CanMonitor));
        OnPropertyChanged(nameof(CanStop));
    }

    private void OnQueryReceived(SqlQuery query)
    {
        Dispatcher.UIThread.Post(() =>
        {
            AllQueries.Insert(0, query);
            TotalQueries = AllQueries.Count;
            ApplyFilter();
        });
    }

    [RelayCommand]
    private async Task StopMonitoringAsync()
    {
        if (_watcher != null)
        {
            await _watcher.StopAsync();
            _watcher.Dispose();
            _watcher = null;
        }

        IsMonitoring = false;
        OnPropertyChanged(nameof(CanMonitor));
        OnPropertyChanged(nameof(CanStop));
        StatusMessage = "Monitoramento parado.";
    }

    [RelayCommand]
    private async Task ExportQueriesAsync()
    {
        if (AllQueries.Count == 0) return;

        var lifetime = Avalonia.Application.Current?.ApplicationLifetime;
        if (lifetime is not Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            StatusMessage = "Export não disponível neste ambiente.";
            return;
        }

        var window = desktop.MainWindow;
        if (window == null) return;

        var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            Title = "Salvar queries capturadas",
            SuggestedFileName = $"sql_queries_{DbName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
            DefaultExtension = "txt",
            FileTypeChoices =
            [
                new Avalonia.Platform.Storage.FilePickerFileType("Arquivo de Texto") { Patterns = ["*.txt"] }
            ]
        });

        if (file == null) return;

        var lines = new List<string>
        {
            $"SQL Queries - Projeto: {ProjectName}",
            $"Database: {DbName} | Host: {DbHost}:{DbPort}",
            $"Exportado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"Total de queries: {AllQueries.Count}",
            new string('-', 80),
            ""
        };

        var ordered = AllQueries.Reverse().ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            var q = ordered[i];
            lines.Add($"[{i + 1}] {q.TimestampDisplay} | {q.DurationDisplay} | {q.Connection}");
            lines.Add($"    {q.Sql}");
            lines.Add("");
        }

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        foreach (var line in lines)
            await writer.WriteLineAsync(line);

        StatusMessage = $"Queries exportadas ({AllQueries.Count}) para {file.Name}";
    }

    [RelayCommand]
    private void ClearQueries()
    {
        AllQueries.Clear();
        FilteredQueries.Clear();
        TotalQueries = 0;
        FilteredCount = 0;
        StatusMessage = "Queries limpas.";
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await StopMonitoringAsync();
        AllQueries.Clear();
        FilteredQueries.Clear();
        IsConnected = false;
        TotalQueries = 0;
        FilteredCount = 0;
        await LoadDbConfigAsync();
    }
}
