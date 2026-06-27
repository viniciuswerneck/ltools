using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.SqlDebugger.Models;

namespace LTools.SqlDebugger.ViewModels;

public partial class SqlDebuggerViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;

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
    private string _statusMessage = "Selecione um projeto no menu lateral.";

    public ObservableCollection<TableInfo> Tables { get; } = [];

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
            _ = LoadTablesAsync();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Tables.Clear();
            DbConnection = string.Empty;
            DbHost = string.Empty;
            DbPort = string.Empty;
            DbName = string.Empty;
            DbUser = string.Empty;
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

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("DB_CONNECTION=")) DbConnection = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_HOST=")) DbHost = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_PORT=")) DbPort = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_DATABASE=")) DbName = trimmed.Split('=', 2)[1];
            else if (trimmed.StartsWith("DB_USERNAME=")) DbUser = trimmed.Split('=', 2)[1];
        }
    }

    private async Task LoadTablesAsync()
    {
        Tables.Clear();
        StatusMessage = "Carregando informações do banco...";

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan db:show --json --no-ansi 2>&1");
            ParseTables(output);
            if (Tables.Count > 0)
                StatusMessage = $"{Tables.Count} tabelas encontradas.";
        }
        catch
        {
            StatusMessage = "Não foi possível listar tabelas (Laravel 11+ necessário para db:show).";
        }
    }

    private void ParseTables(string json)
    {
        try
        {
            var start = json.IndexOf('{');
            if (start < 0) return;
            json = json[start..];

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("tables", out var tables))
            {
                foreach (var table in tables.EnumerateArray())
                {
                    Tables.Add(new TableInfo
                    {
                        Name = GetString(table, "name"),
                        Engine = GetString(table, "engine"),
                        Rows = GetString(table, "rows_count"),
                        Size = GetString(table, "data_size"),
                        Comment = GetString(table, "comment")
                    });
                }
            }
        }
        catch { }
    }

    private static string GetString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var prop) ? prop.GetString() ?? "" : "";
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDbConfigAsync();
        await LoadTablesAsync();
    }
}