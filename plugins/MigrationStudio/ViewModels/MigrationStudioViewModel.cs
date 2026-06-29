using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.MigrationStudio.Models;
using LTools.MigrationStudio.Views;

namespace LTools.MigrationStudio.ViewModels;

public partial class MigrationStudioViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _tableName = string.Empty;

    [ObservableProperty]
    private ColumnDefinition? _selectedColumn;

    [ObservableProperty]
    private string _generatedCode = string.Empty;

    [ObservableProperty]
    private bool _timestamps = true;

    [ObservableProperty]
    private bool _softDeletes;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    public ObservableCollection<ColumnDefinition> Columns { get; } = [];
    public ObservableCollection<MigrationInfo> MigrationList { get; } = [];

    [ObservableProperty]
    private MigrationInfo? _selectedMigration;

    public string[] TiposColuna { get; } =
    [
        "bigIncrements", "increments", "id",
        "string", "char", "text", "mediumText", "longText",
        "integer", "bigInteger", "mediumInteger", "smallInteger", "tinyInteger",
        "float", "double", "decimal",
        "boolean",
        "date", "datetime", "timestamp", "time", "year",
        "enum", "json", "jsonb",
        "foreignId", "uuid"
    ];

    public string[] OpcoesOnDelete { get; } =
        ["cascade", "set null", "restrict", "no action"];

    public MigrationStudioViewModel()
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
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Columns.Clear();
            MigrationList.Clear();
            SelectedMigration = null;
            TableName = string.Empty;
            GeneratedCode = string.Empty;
            InitFromContext();
            StatusMessage = !string.IsNullOrWhiteSpace(_projectPath)
                ? "Pronto. Clique em Carregar Migrations."
                : "Selecione um projeto na barra superior.";
        });
    }

    partial void OnTableNameChanged(string value)
    {
        RegenerateCode();
    }

    partial void OnSelectedColumnChanged(ColumnDefinition? value)
    {
        RegenerateCode();
    }

    partial void OnTimestampsChanged(bool value) => RegenerateCode();
    partial void OnSoftDeletesChanged(bool value) => RegenerateCode();

    private void OnColumnChanged()
    {
        RegenerateCode();
        OnPropertyChanged(nameof(CanGenerate));
    }

    [RelayCommand]
    private void AddColumn()
    {
        var col = new ColumnDefinition();
        col.PropertyChanged += (_, _) => OnColumnChanged();
        Columns.Add(col);
        SelectedColumn = col;
        OnColumnChanged();
    }

    [RelayCommand]
    private void RemoveColumn()
    {
        if (SelectedColumn == null) return;
        var idx = Columns.IndexOf(SelectedColumn);
        Columns.Remove(SelectedColumn);
        SelectedColumn = Columns.Count > 0
            ? Columns[Math.Min(idx, Columns.Count - 1)]
            : null;
        OnColumnChanged();
    }

    [RelayCommand]
    private void MoveColumnUp()
    {
        if (SelectedColumn == null) return;
        var idx = Columns.IndexOf(SelectedColumn);
        if (idx <= 0) return;
        Columns.Move(idx, idx - 1);
        OnColumnChanged();
    }

    [RelayCommand]
    private void MoveColumnDown()
    {
        if (SelectedColumn == null) return;
        var idx = Columns.IndexOf(SelectedColumn);
        if (idx < 0 || idx >= Columns.Count - 1) return;
        Columns.Move(idx, idx + 1);
        OnColumnChanged();
    }

    public bool CanGenerate => Columns.Count > 0 || Timestamps || SoftDeletes;

    public void RegenerateCode()
    {
        var table = TableName?.Trim().ToLower().Replace(' ', '_') ?? "";
        if (string.IsNullOrWhiteSpace(table) || !CanGenerate)
        {
            GeneratedCode = string.Empty;
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<?php");
        sb.AppendLine();
        sb.AppendLine("use Illuminate\\Database\\Migrations\\Migration;");
        sb.AppendLine("use Illuminate\\Database\\Schema\\Blueprint;");
        sb.AppendLine("use Illuminate\\Support\\Facades\\Schema;");
        sb.AppendLine();
        sb.AppendLine("return new class extends Migration");
        sb.AppendLine("{");
        sb.AppendLine("    public function up(): void");
        sb.AppendLine("    {");
        sb.AppendLine($"        Schema::create('{table}', function (Blueprint $table) {{");

        foreach (var col in Columns)
        {
            if (string.IsNullOrWhiteSpace(col.Name)) continue;
            var line = BuildColumnLine(col);
            sb.AppendLine($"            {line}");
        }

        if (Timestamps)
            sb.AppendLine("            $table->timestamps();");
        if (SoftDeletes)
            sb.AppendLine("            $table->softDeletes();");

        sb.AppendLine("        });");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public function down(): void");
        sb.AppendLine("    {");
        sb.AppendLine($"        Schema::dropIfExists('{table}');");
        sb.AppendLine("    }");
        sb.AppendLine("};");

        GeneratedCode = sb.ToString();
    }

    private static string BuildColumnLine(ColumnDefinition col)
    {
        var type = col.Type;
        var name = col.Name.Trim();

        var call = type switch
        {
            "id" => "$table->id()",
            "bigIncrements" => $"$table->bigIncrements('{name}')",
            "increments" => $"$table->increments('{name}')",
            "string" => $"$table->string('{name}', {col.Length})",
            "char" => $"$table->char('{name}', {col.Length})",
            "decimal" => $"$table->decimal('{name}', {col.Precision}, {col.Scale})",
            "float" => $"$table->float('{name}', {col.Precision})",
            "double" => $"$table->double('{name}', {col.Precision})",
            "enum" when !string.IsNullOrWhiteSpace(col.EnumValues) =>
                $"$table->enum('{name}', [{string.Join(", ", col.EnumValues.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(v => $"'{v}'"))}])",
            "foreignId" => $"$table->foreignId('{name}')",
            _ => $"$table->{type}('{name}')"
        };

        if (col.Unsigned && type is "integer" or "bigInteger" or "mediumInteger" or "smallInteger" or "tinyInteger" or "float" or "double" or "decimal")
            call += "->unsigned()";

        var isAutoIncrementType = type is "id" or "bigIncrements" or "increments";
        var isUserAuto = col.AutoIncrement && !isAutoIncrementType && type is "integer" or "bigInteger" or "mediumInteger";
        var isAuto = isAutoIncrementType || isUserAuto;

        if (isAuto)
            call += "->autoIncrement()";

        if (col.Primary && type != "id")
            call += "->primary()";

        if (col.Nullable && !isAuto)
            call += "->nullable()";

        if (col.Unique)
            call += "->unique()";

        if (!string.IsNullOrWhiteSpace(col.DefaultValue) && !isAuto)
            call += $"->default('{col.DefaultValue}')";

        if (!string.IsNullOrWhiteSpace(col.Comment))
            call += $"->comment('{col.Comment}')";

        if (col.Foreign && !string.IsNullOrWhiteSpace(col.ForeignTable))
        {
            if (type == "foreignId")
                call += $"->constrained('{col.ForeignTable}')";
            else
            {
                var fkCol = string.IsNullOrWhiteSpace(col.ForeignColumn) ? "id" : col.ForeignColumn;
                call += $";\n            $table->foreign('{name}')->references('{fkCol}')->on('{col.ForeignTable}')";
                if (col.OnDelete != "cascade")
                    call += $"->onDelete('{col.OnDelete}')";
                if (col.OnUpdate != "cascade")
                    call += $"->onUpdate('{col.OnUpdate}')";
            }
        }

        return call + ";";
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim('_');
    }

    [RelayCommand]
    private async Task SaveMigrationAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        if (string.IsNullOrWhiteSpace(GeneratedCode))
        {
            StatusMessage = "Nada para salvar. Adicione campos primeiro.";
            return;
        }

        var table = TableName.Trim().ToLower().Replace(' ', '_');
        var now = DateTime.Now;
        var timestamp = $"{now:yyyy_MM_dd_HHmmss}";
        var safeTable = SanitizeFileName(table);
        var fileName = $"{timestamp}_create_{safeTable}_table.php";
        var migrationsDir = Path.Combine(_projectPath, "database", "migrations");
        var filePath = Path.Combine(migrationsDir, fileName);

        try
        {
            Directory.CreateDirectory(migrationsDir);
            await File.WriteAllTextAsync(filePath, GeneratedCode);
            StatusMessage = $"Migration salva: {fileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao salvar: {ex.Message}";
        }
    }

    private async Task RunArtisanAsync(string args, string successMsg)
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        IsRunning = true;
        OutputText = string.Empty;
        StatusMessage = $"Executando php artisan {args}...";

        try
        {
            _runner.OutputReceived += OnOutput;
            _runner.ErrorReceived += OnError;
            var exitCode = await _runner.RunAsync(_projectPath, "php", $"artisan {args}");
            if (exitCode == 0)
            {
                StatusMessage = successMsg;
                // Auto-refresh migration list after successful operation
                await LoadMigrationsAsync();
            }
            else
            {
                StatusMessage = $"Comando falhou (código {exitCode}). Veja o log acima.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }
        finally
        {
            _runner.OutputReceived -= OnOutput;
            _runner.ErrorReceived -= OnError;
            IsRunning = false;
        }
    }

    [RelayCommand]
    private Task MigrateAsync() => RunArtisanAsync("migrate", "Migration executada com sucesso!");

    [RelayCommand]
    private Task RollbackAsync() => RunArtisanAsync("migrate:rollback", "Rollback executado com sucesso!");

    [RelayCommand]
    private void TestButton()
    {
        StatusMessage = $"Teste: _projectPath='{_projectPath}', IsRunning={IsRunning}, hora={DateTime.Now:HH:mm:ss}";
    }

    [RelayCommand]
    private async Task LoadMigrationsAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }
        IsRunning = true;
        StatusMessage = "Carregando migrations...";
        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan migrate:status");
            if (string.IsNullOrWhiteSpace(output))
            {
                StatusMessage = "Nenhuma saída do artisan. Verifique se o PHP está no PATH.";
                return;
            }
            ParseMigrationStatus(output);
            StatusMessage = $"{MigrationList.Count} migrations encontradas.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }

    private static string StripAnsi(string input)
    {
        return Regex.Replace(input, @"\x1B\[[0-9;]*[a-zA-Z]", "");
    }

    private void ParseMigrationStatus(string output)
    {
        MigrationList.Clear();
        var clean = StripAnsi(output);

        // Try new format (Laravel 11+: "  name .......... [1] Ran" / "  name .......... Pending")
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
                MigrationList.Add(new MigrationInfo
                {
                    Name = match.Groups[1].Value.Trim(),
                    Ran = true,
                    Batch = int.Parse(match.Groups[2].Value)
                });
                continue;
            }

            match = Regex.Match(trimmed, @"^(.+?)\s+\.+\s+Pending$");
            if (match.Success)
            {
                MigrationList.Add(new MigrationInfo
                {
                    Name = match.Groups[1].Value.Trim(),
                    Ran = false,
                    Batch = null
                });
            }
        }

        if (MigrationList.Count > 0) return;

        // Fallback: old format (Laravel 10-: pipe table)
        foreach (var line in clean.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith('|') || trimmed.Contains("Ran?") || trimmed.Contains("Migration") || trimmed.Contains("+--")) continue;

            var parts = trimmed.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) continue;

            MigrationList.Add(new MigrationInfo
            {
                Name = parts[2],
                Ran = parts[1].Equals("Yes", StringComparison.OrdinalIgnoreCase),
                Batch = int.TryParse(parts[3], out var b) ? b : null
            });
        }
    }

    [RelayCommand]
    private async Task RollbackToSelectedAsync()
    {
        if (SelectedMigration == null || !SelectedMigration.Ran || SelectedMigration.Batch == null)
        {
            StatusMessage = "Selecione uma migration já executada para rollback.";
            return;
        }
        await RunArtisanAsync($"migrate:rollback --batch={SelectedMigration.Batch}",
            $"Rollback até {SelectedMigration.Name} concluído!");
    }

    [RelayCommand]
    private Task FreshAsync() => RunArtisanAsync("migrate:fresh", "Fresh executado com sucesso!");

    [RelayCommand]
    private Task RefreshAsync() => RunArtisanAsync("migrate:refresh", "Refresh executado com sucesso!");

    [RelayCommand]
    private void ClearOutput()
    {
        OutputText = string.Empty;
        StatusMessage = "Log limpo.";
    }

    [RelayCommand]
    private void CompareMigrations()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        var window = new CompareMigrationsWindow
        {
            DataContext = new CompareMigrationsViewModel(_projectPath)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            window.ShowDialog(desktop.MainWindow);
    }

    private void OnOutput(string data)
    {
        Dispatcher.UIThread.Post(() => OutputText += data + "\n");
    }

    private void OnError(string data)
    {
        Dispatcher.UIThread.Post(() => OutputText += $"[ERRO] {data}\n");
    }
}
