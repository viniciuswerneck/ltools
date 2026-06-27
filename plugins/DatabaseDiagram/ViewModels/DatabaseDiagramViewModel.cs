using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private string _statusMessage = "Selecione um projeto Laravel para analisar o banco.";

    public ObservableCollection<TableSchema> Tables { get; } = [];

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
        ProjectName = Path.GetFileName(_projectPath);

        await AnalyzeMigrationsAsync();
    }

    private async Task AnalyzeMigrationsAsync()
    {
        Tables.Clear();
        StatusMessage = "Analisando migrations...";

        var migrationsDir = Path.Combine(_projectPath, "database", "migrations");
        if (!Directory.Exists(migrationsDir))
        {
            StatusMessage = "Diretório database/migrations não encontrado.";
            return;
        }

        await Task.Run(() =>
        {
            var migrationFiles = Directory.GetFiles(migrationsDir, "*.php")
                .OrderBy(f => f)
                .ToList();

            foreach (var file in migrationFiles)
            {
                var content = File.ReadAllText(file);
                ParseMigration(content);
            }
        });

        DetectRelationships();
        StatusMessage = $"{Tables.Count} tabelas encontradas nas migrations.";
    }

    private void ParseMigration(string content)
    {
        var schemaMatch = Regex.Match(content, @"Schema::create\s*\(\s*['""]([\w_]+)['""]\s*,", RegexOptions.Singleline);
        if (!schemaMatch.Success) return;

        var tableName = schemaMatch.Groups[1].Value;
        var table = new TableSchema { Name = tableName };

        var columnMatches = Regex.Matches(content,
            @"\$table->(\w+)\s*\(\s*['""]([\w_]+)['""]([^)]*)\)",
            RegexOptions.Singleline);

        foreach (Match match in columnMatches)
        {
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var rest = match.Groups[3].Value;

            var isPrimary = type == "id" || rest.Contains("->primary()") || name == "id";
            var isNullable = rest.Contains("->nullable()");
            var foreignKey = "";

            var fkMatch = Regex.Match(rest, @"->constrained\(\)|->references\(['""]([\w_]+)['""]\)->on\(['""]([\w_]+)['""]\)");
            if (fkMatch.Success)
            {
                foreignKey = fkMatch.Groups[2].Success
                    ? $"{fkMatch.Groups[2].Value}.{fkMatch.Groups[1].Value}"
                    : "constrained";
            }

            table.Columns.Add(new ColumnSchema
            {
                Name = name,
                Type = type,
                IsPrimary = isPrimary,
                IsNullable = isNullable,
                ForeignKey = foreignKey
            });
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() => Tables.Add(table));
    }

    private void DetectRelationships()
    {
        foreach (var table in Tables)
        {
            foreach (var col in table.Columns)
            {
                if (!string.IsNullOrWhiteSpace(col.ForeignKey))
                {
                    var parts = col.ForeignKey.Split('.');
                    var targetTable = parts[0];
                    var targetCol = parts.Length > 1 ? parts[1] : "id";

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
    }

    partial void OnSelectedTableChanged(TableSchema? value)
    {
        if (value != null)
            StatusMessage = $"Tabela: {value.Name} | {value.Columns.Count} colunas | {value.Relationships.Count} relacionamentos";
    }
}