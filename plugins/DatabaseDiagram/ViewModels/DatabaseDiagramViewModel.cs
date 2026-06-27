using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.DatabaseDiagram.Models;

namespace LTools.DatabaseDiagram.ViewModels;

public partial class DatabaseDiagramViewModel : ObservableObject
{
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private TableSchema? _selectedTable;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    public ObservableCollection<TableSchema> Tables { get; } = [];

    public DatabaseDiagramViewModel()
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
            _ = AnalyzeMigrationsAsync();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Tables.Clear();
            SelectedTable = null;
            InitFromContext();
        });
    }

    private async Task AnalyzeMigrationsAsync()
    {
        Tables.Clear();
        SelectedTable = null;
        StatusMessage = "Analisando migrations...";

        var migrationsDir = Path.Combine(_projectPath, "database", "migrations");
        if (!Directory.Exists(migrationsDir))
        {
            StatusMessage = "Diretório database/migrations não encontrado.";
            return;
        }

        List<TableSchema> parsedTables;

        try
        {
            parsedTables = await Task.Run(() =>
            {
                var migrationFiles = Directory.GetFiles(migrationsDir, "*.php")
                    .OrderBy(f => f)
                    .ToList();

                var allTables = new List<TableSchema>();

                foreach (var file in migrationFiles)
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        ParseMigration(content, allTables);
                    }
                    catch { }
                }

                return allTables;
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao analisar migrations: {ex.Message}";
            return;
        }

        foreach (var t in parsedTables)
            Tables.Add(t);

        DetectRelationships();
        StatusMessage = $"{Tables.Count} tabelas encontradas nas migrations.";
    }

    private static void ParseMigration(string content, List<TableSchema> allTables)
    {
        var tableName = "";

        var createMatch = Regex.Match(content, @"Schema::create\s*\(\s*['""]([\w_]+)['""]\s*,", RegexOptions.Singleline);
        if (createMatch.Success)
            tableName = createMatch.Groups[1].Value;
        else
        {
            var tableMatch = Regex.Match(content, @"Schema::table\s*\(\s*['""]([\w_]+)['""]\s*,", RegexOptions.Singleline);
            if (!tableMatch.Success) return;
            tableName = tableMatch.Groups[1].Value;
        }

        var existingTable = allTables.FirstOrDefault(t => t.Name == tableName);
        var table = existingTable ?? new TableSchema { Name = tableName };

        var columnMatches = Regex.Matches(content,
            @"\$table->(\w+)\s*\(\s*(?:['""]([\w_]+)['""]\s*)?([^;]*);",
            RegexOptions.Singleline);

        foreach (Match match in columnMatches)
        {
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Success ? match.Groups[2].Value : "";
            var rest = match.Groups[3].Value;

            var isPrimary = type == "id" || rest.Contains("->primary()");
            var isNullable = rest.Contains("->nullable()");
            var foreignKey = "";

            var fkMatch = Regex.Match(rest, @"->constrained\(\s*['""]?([\w_]*)['""]?\s*\)|->references\(['""]([\w_]+)['""]\)\s*->\s*on\s*\(\s*['""]([\w_]+)['""]\s*\)");
            if (fkMatch.Success)
            {
                if (!string.IsNullOrWhiteSpace(fkMatch.Groups[1].Value))
                    foreignKey = $"{fkMatch.Groups[1].Value}.id";
                else if (!string.IsNullOrWhiteSpace(fkMatch.Groups[2].Value))
                    foreignKey = $"{fkMatch.Groups[3].Value}.{fkMatch.Groups[2].Value}";
                else
                    foreignKey = "constrained";
            }
            else if (name.EndsWith("_id") && type == "foreignId")
            {
                var inferred = name[..^3] + "s";
                foreignKey = $"{inferred}.id";
            }

            if (table.Columns.All(c => c.Name != name || name == ""))
            {
                table.Columns.Add(new ColumnSchema
                {
                    Name = name,
                    Type = type,
                    IsPrimary = isPrimary,
                    IsNullable = isNullable,
                    ForeignKey = foreignKey
                });
            }
        }

        if (existingTable == null)
            allTables.Add(table);
    }

    private void DetectRelationships()
    {
        foreach (var table in Tables)
        {
            foreach (var col in table.Columns)
            {
                if (string.IsNullOrWhiteSpace(col.ForeignKey)) continue;

                var targetTable = col.ForeignKey;
                var targetCol = "id";

                if (targetTable == "constrained")
                {
                    if (!col.Name.EndsWith("_id")) continue;
                    targetTable = col.Name[..^3] + "s";
                }
                else
                {
                    var parts = targetTable.Split('.');
                    targetTable = parts[0];
                    targetCol = parts.Length > 1 ? parts[1] : "id";
                }

                var relTable = Tables.FirstOrDefault(t =>
                    t.Name.Equals(targetTable, StringComparison.OrdinalIgnoreCase));
                if (relTable == null) continue;

                table.Relationships.Add(new Relationship
                {
                    FromTable = table.Name,
                    FromColumn = col.Name,
                    ToTable = relTable.Name,
                    ToColumn = targetCol
                });
            }
        }
    }

    partial void OnSelectedTableChanged(TableSchema? value)
    {
        if (value != null)
            StatusMessage = $"Tabela: {value.Name} | {value.Columns.Count} colunas | {value.Relationships.Count} relacionamentos";
    }
}