using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.MigrationStudio.Models;

namespace LTools.MigrationStudio.ViewModels;

public partial class CompareMigrationsViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private readonly string _projectPath;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private int _ranCount;

    [ObservableProperty]
    private int _orphanTableCount;

    public bool HasPending => PendingCount > 0;
    public bool HasOrphans => OrphanTableCount > 0;
    public bool HasDivergences => PendingCount > 0 || OrphanTableCount > 0;

    partial void OnPendingCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasPending));
        OnPropertyChanged(nameof(HasDivergences));
    }

    partial void OnOrphanTableCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasOrphans));
        OnPropertyChanged(nameof(HasDivergences));
    }

    public ObservableCollection<CompareMigrationItem> Items { get; } = [];
    public ObservableCollection<CompareMigrationItem> PendingMigrations { get; } = [];
    public ObservableCollection<CompareMigrationItem> OrphanTables { get; } = [];
    public ObservableCollection<CompareMigrationItem> AllMigrations { get; } = [];

    public CompareMigrationsViewModel(string projectPath)
    {
        _projectPath = projectPath;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Nenhum projeto selecionado.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Analisando migrations e tabelas...";

        try
        {
            Items.Clear();
            PendingMigrations.Clear();
            OrphanTables.Clear();
            AllMigrations.Clear();

            var dbTables = await GetDatabaseTablesAsync();
            var (ranMigrations, pendingMigrations) = await GetMigrationStatusAsync();
            var migrationFiles = GetMigrationFiles();

            foreach (var m in ranMigrations)
                AllMigrations.Add(m);

            foreach (var m in pendingMigrations)
            {
                AllMigrations.Add(m);
                PendingMigrations.Add(m);
                Items.Add(m);
            }

            var createdTables = ExtractCreatedTables(migrationFiles);
            var orphanTables = dbTables
                .Where(t => !createdTables.Contains(t, StringComparer.OrdinalIgnoreCase))
                .OrderBy(t => t)
                .ToList();

            foreach (var table in orphanTables)
            {
                var item = new CompareMigrationItem
                {
                    Name = table,
                    TypeLabel = "Tabela",
                    StatusText = "Sem migration correspondente",
                    StatusIcon = "🆕",
                    Detail = "Tabela encontrada no banco mas sem migration create"
                };
                OrphanTables.Add(item);
                Items.Add(item);
            }

            PendingCount = pendingMigrations.Count;
            OrphanTableCount = orphanTables.Count;
            RanCount = ranMigrations.Count;

            if (Items.Count == 0)
                StatusMessage = "Nenhuma divergência encontrada. Tudo ok!";
            else
                StatusMessage = $"{Items.Count} divergência(s) — {PendingCount} pendente(s), {OrphanTableCount} tabela(s) órfã(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro na análise: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private List<CompareMigrationItem> GetMigrationFiles()
    {
        var result = new List<CompareMigrationItem>();
        var migrationsDir = Path.Combine(_projectPath, "database", "migrations");

        if (!Directory.Exists(migrationsDir))
            return result;

        foreach (var file in Directory.GetFiles(migrationsDir, "*.php"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name == null) continue;
            result.Add(new CompareMigrationItem
            {
                Name = name,
                TypeLabel = "Migration",
                StatusText = "Arquivo encontrado",
                StatusIcon = "📄",
                Detail = "Migration file on disk"
            });
        }

        return result;
    }

    private async Task<(List<CompareMigrationItem> ran, List<CompareMigrationItem> pending)> GetMigrationStatusAsync()
    {
        var ran = new List<CompareMigrationItem>();
        var pending = new List<CompareMigrationItem>();

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan migrate:status");
            if (string.IsNullOrWhiteSpace(output))
            {
                StatusMessage = "Nenhuma saída do artisan migrate:status.";
                return (ran, pending);
            }

            ParseStatus(output, ran, pending);
        }
        catch
        {
            StatusMessage = "Não foi possível obter o status das migrations.";
        }

        return (ran, pending);
    }

    private static void ParseStatus(string output, List<CompareMigrationItem> ran, List<CompareMigrationItem> pending)
    {
        var clean = StripAnsi(output);

        // Laravel 11+ format: "  name .......... [1] Ran" / "  name .......... Pending"
        var newFormat = false;
        foreach (var line in clean.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.Contains("Batch / Status") || trimmed.Contains("Migration name"))
            {
                newFormat = true;
                continue;
            }
            if (!newFormat) continue;

            var match = Regex.Match(trimmed, @"^(.+?)\s+\.+\s+\[(\d+)\]\s+Ran$");
            if (match.Success)
            {
                ran.Add(new CompareMigrationItem
                {
                    Name = match.Groups[1].Value.Trim(),
                    TypeLabel = "Migration",
                    StatusText = "Executada",
                    StatusIcon = "✅",
                    Batch = match.Groups[2].Value
                });
                continue;
            }

            match = Regex.Match(trimmed, @"^(.+?)\s+\.+\s+Pending$");
            if (match.Success)
            {
                pending.Add(new CompareMigrationItem
                {
                    Name = match.Groups[1].Value.Trim(),
                    TypeLabel = "Migration",
                    StatusText = "Pendente",
                    StatusIcon = "⏳",
                    Detail = "Migration não executada"
                });
            }
        }

        if (ran.Count > 0 || pending.Count > 0) return;

        // Fallback: Laravel 10- format (pipe table)
        foreach (var line in clean.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith('|') || trimmed.Contains("Ran?") || trimmed.Contains("Migration") || trimmed.Contains("+--")) continue;

            var parts = trimmed.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) continue;

            var isRan = parts[1].Equals("Yes", StringComparison.OrdinalIgnoreCase);
            var item = new CompareMigrationItem
            {
                Name = parts[2],
                TypeLabel = "Migration",
                StatusText = isRan ? "Executada" : "Pendente",
                StatusIcon = isRan ? "✅" : "⏳",
                Batch = isRan && int.TryParse(parts[3], out var b) ? b.ToString() : null,
                Detail = isRan ? null : "Migration não executada"
            };

            if (isRan) ran.Add(item);
            else pending.Add(item);
        }
    }

    private async Task<List<string>> GetDatabaseTablesAsync()
    {
        var tables = new List<string>();

        try
        {
            // Try JSON format first (Laravel 9+)
            var jsonOutput = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan db:show --json");
            if (!string.IsNullOrWhiteSpace(jsonOutput))
                tables = ParseDbShowJson(jsonOutput);

            if (tables.Count > 0)
                return tables;
        }
        catch
        {
            // Fall through to table format
        }

        try
        {
            // Fallback: parse table format (Laravel 8+)
            var tableOutput = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan db:show");
            if (!string.IsNullOrWhiteSpace(tableOutput))
                tables = ParseDbShowTable(tableOutput);
        }
        catch
        {
            StatusMessage = "Não foi possível listar as tabelas do banco. Verifique se o projeto está configurado.";
        }

        return tables;
    }

    private static List<string> ParseDbShowJson(string json)
    {
        var tables = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("tables", out var tablesEl))
            {
                foreach (var table in tablesEl.EnumerateArray())
                {
                    if (table.TryGetProperty("name", out var nameEl))
                    {
                        var name = nameEl.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                            tables.Add(name);
                    }
                }
            }
        }
        catch
        {
            // Return what we have
        }

        return tables;
    }

    private static List<string> ParseDbShowTable(string output)
    {
        var clean = StripAnsi(output);
        var tables = new List<string>();
        var inTable = false;

        foreach (var line in clean.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("+--"))
            {
                inTable = !inTable;
                continue;
            }

            if (!inTable) continue;
            if (!trimmed.StartsWith('|')) continue;

            var parts = trimmed.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length < 2) continue;

            var name = parts[0];
            if (!string.IsNullOrWhiteSpace(name) && !name.Contains("Table") && !name.Contains("---"))
                tables.Add(name);
        }

        return tables;
    }

    private static HashSet<string> ExtractCreatedTables(List<CompareMigrationItem> migrations)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "migrations" // always exclude the migrations system table
        };

        foreach (var m in migrations)
        {
            var match = Regex.Match(m.Name, @"^.*_create_(\w+)_table$");
            if (match.Success)
                tables.Add(match.Groups[1].Value);
        }

        return tables;
    }

    private static string StripAnsi(string input)
    {
        return Regex.Replace(input, @"\x1B\[[0-9;]*[a-zA-Z]", "");
    }
}
