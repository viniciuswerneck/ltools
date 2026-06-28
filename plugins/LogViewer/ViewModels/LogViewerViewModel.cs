using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.LogViewer.Models;

namespace LTools.LogViewer.ViewModels;

public partial class LogViewerViewModel : ObservableObject
{
    private string _projectPath = string.Empty;
    private FileSystemWatcher? _watcher;
    private List<LogEntry> _allEntries = [];
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _logContent = string.Empty;

    [ObservableProperty]
    private string _selectedLogName = string.Empty;

    [ObservableProperty]
    private bool _autoRefresh;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedLevel = "Todos";

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private bool _isRawView;

    public bool IsStructuredView => !IsRawView;

    [ObservableProperty]
    private LogEntry? _selectedEntry;

    public bool HasSelectedEntry => SelectedEntry != null;
    public bool ShowPlaceholder => !HasSelectedEntry;
    public bool ShowEntryCount => !IsRawView;

    [ObservableProperty]
    private int _totalEntries;

    [ObservableProperty]
    private int _filteredCount;

    [ObservableProperty]
    private string _detailContent = string.Empty;

    public ObservableCollection<LogFile> LogFiles { get; } = [];
    public ObservableCollection<LogEntry> FilteredEntries { get; } = [];
    public ObservableCollection<string> LevelFilters { get; } = ["Todos", "ERROR", "WARNING", "INFO", "DEBUG", "CRITICAL", "ALERT", "EMERGENCY", "NOTICE"];

    public LogViewerViewModel()
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
            var logsDir = Path.Combine(_projectPath, "storage", "logs");
            if (Directory.Exists(logsDir))
            {
                StartWatching(logsDir);
                _ = LoadLogFilesAsync();
            }
            else
            {
                StatusMessage = "Diretório storage/logs não encontrado.";
            }
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _watcher?.Dispose();
            _watcher = null;
            LogFiles.Clear();
            LogContent = string.Empty;
            SelectedLogName = string.Empty;
            FilteredEntries.Clear();
            _allEntries.Clear();
            _currentFilePath = string.Empty;
            TotalEntries = 0;
            FilteredCount = 0;
            DetailContent = string.Empty;
            SelectedEntry = null;
            InitFromContext();
        });
    }

    private void StartWatching(string logsDir)
    {
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(logsDir, "*.log")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _watcher.Created += async (_, _) => await LoadLogFilesAsync();
        _watcher.Deleted += async (_, _) => await LoadLogFilesAsync();
        _watcher.Changed += async (_, e) =>
        {
            if (AutoRefresh && SelectedLogName == e.Name)
                await LoadLogContentAsync(e.FullPath);
        };
        _watcher.EnableRaisingEvents = true;
    }

    private async Task LoadLogFilesAsync()
    {
        LogFiles.Clear();
        var logsDir = Path.Combine(_projectPath, "storage", "logs");
        if (!Directory.Exists(logsDir)) return;

        await Task.Run(() =>
        {
            foreach (var file in Directory.GetFiles(logsDir, "*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    LogFiles.Add(new LogFile
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        Size = file.Length,
                        LastModified = file.LastWriteTime
                    });
                });
            }
        });

        StatusMessage = $"{LogFiles.Count} arquivo(s) de log encontrado(s).";
    }

    [RelayCommand]
    private async Task OpenLogAsync(LogFile? logFile)
    {
        if (logFile == null) return;
        SelectedLogName = logFile.Name;
        _currentFilePath = logFile.FullPath;
        await LoadLogContentAsync(logFile.FullPath);
    }

    private async Task LoadLogContentAsync(string fullPath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(fullPath);
            LogContent = content;

            _allEntries = ParseLogContent(content);
            TotalEntries = _allEntries.Count;
            ApplyFilters();

            StatusMessage = $"Visualizando: {Path.GetFileName(fullPath)} ({TotalEntries} entradas)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao ler log: {ex.Message}";
        }
    }

    private List<LogEntry> ParseLogContent(string content)
    {
        var entries = new List<LogEntry>();
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var entryRegex = new Regex(@"^\[(\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:\.\d+)?)\] (\w+)\.(\w+): (.*)");

        LogEntry? currentEntry = null;
        int lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;
            var match = entryRegex.Match(line);
            if (match.Success)
            {
                if (currentEntry != null)
                    entries.Add(currentEntry);

                currentEntry = new LogEntry
                {
                    LineNumber = lineNumber,
                    Timestamp = DateTime.TryParse(match.Groups[1].Value.Trim(), out var dt) ? dt : null,
                    Environment = match.Groups[2].Value,
                    Level = match.Groups[3].Value.ToUpperInvariant(),
                    Message = match.Groups[4].Value,
                    FullContent = line
                };
            }
            else if (currentEntry != null)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    currentEntry.StackTrace += (currentEntry.StackTrace.Length > 0 ? "\n" : "") + line;
                    currentEntry.FullContent += "\n" + line;
                }
            }
        }

        if (currentEntry != null)
            entries.Add(currentEntry);

        return entries;
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedLevelChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnStartDateChanged(DateTime? value)
    {
        ApplyFilters();
    }

    partial void OnEndDateChanged(DateTime? value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allEntries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(e =>
                e.Message.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.StackTrace.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (e.Timestamp?.ToString("yyyy-MM-dd HH:mm:ss").Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (SelectedLevel != "Todos")
        {
            filtered = filtered.Where(e => e.Level == SelectedLevel);
        }

        if (StartDate.HasValue)
        {
            var start = StartDate.Value.Date;
            filtered = filtered.Where(e => e.Timestamp.HasValue && e.Timestamp.Value >= start);
        }

        if (EndDate.HasValue)
        {
            var end = EndDate.Value.Date.AddDays(1);
            filtered = filtered.Where(e => e.Timestamp.HasValue && e.Timestamp.Value < end);
        }

        var result = filtered.ToList();

        FilteredEntries.Clear();
        foreach (var entry in result)
        {
            FilteredEntries.Add(entry);
        }

        FilteredCount = FilteredEntries.Count;
        OnPropertyChanged(nameof(FilteredEntries));

        if (SelectedEntry != null && !FilteredEntries.Contains(SelectedEntry))
        {
            SelectedEntry = null;
            DetailContent = string.Empty;
        }
    }

    partial void OnSelectedEntryChanged(LogEntry? value)
    {
        OnPropertyChanged(nameof(HasSelectedEntry));
        OnPropertyChanged(nameof(ShowPlaceholder));
        if (value == null)
        {
            DetailContent = string.Empty;
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[{value.FormattedTimestamp}] {value.Environment}.{value.Level}");
        sb.AppendLine();
        sb.AppendLine(value.Message);
        if (value.HasStackTrace)
        {
            sb.AppendLine();
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(value.StackTrace);
        }
        DetailContent = sb.ToString().TrimEnd();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedLevel = "Todos";
        StartDate = null;
        EndDate = null;
    }

    private async Task CopyToClipboardAsync(string text, string successMessage)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        try
        {
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime;
            if (lifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                    StatusMessage = successMessage;
                    return;
                }
            }
            StatusMessage = "Não foi possível acessar a área de transferência.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao copiar: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CopyFilteredAsync()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var entry in FilteredEntries)
        {
            sb.AppendLine(entry.FullContent);
        }

        if (sb.Length > 0)
        {
            await CopyToClipboardAsync(sb.ToString().TrimEnd(),
                $"{FilteredEntries.Count} entrada(s) copiada(s) para a área de transferência.");
        }
    }

    [RelayCommand]
    private async Task CopyEntryAsync()
    {
        if (SelectedEntry == null) return;
        await CopyToClipboardAsync(SelectedEntry.FullContent,
            "Entrada copiada para a área de transferência.");
    }

    [RelayCommand]
    private async Task CopyRawAsync()
    {
        await CopyToClipboardAsync(LogContent,
            "Conteúdo completo copiado para a área de transferência.");
    }

    [RelayCommand]
    private void ToggleRawView()
    {
        IsRawView = !IsRawView;
        StatusMessage = IsRawView ? "Visualização raw" : "Visualização estruturada";
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadLogFilesAsync();
        if (!string.IsNullOrWhiteSpace(SelectedLogName))
        {
            var log = LogFiles.FirstOrDefault(l => l.Name == SelectedLogName);
            if (log != null)
                await LoadLogContentAsync(log.FullPath);
        }
    }

    [RelayCommand]
    private void ClearLog()
    {
        LogContent = string.Empty;
        FilteredEntries.Clear();
        _allEntries.Clear();
        TotalEntries = 0;
        FilteredCount = 0;
        DetailContent = string.Empty;
        SelectedEntry = null;
        StatusMessage = "Visualização limpa.";
    }

    partial void OnAutoRefreshChanged(bool value)
    {
        StatusMessage = value ? "Auto-refresh ativado" : "Auto-refresh desativado";
    }

    partial void OnIsRawViewChanged(bool value)
    {
        OnPropertyChanged(nameof(IsStructuredView));
        OnPropertyChanged(nameof(ShowEntryCount));
    }
}
