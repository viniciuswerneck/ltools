using System.Collections.ObjectModel;
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

    public ObservableCollection<LogFile> LogFiles { get; } = [];

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
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
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

        StatusMessage = $"{LogFiles.Count} arquivos de log encontrados.";
    }

    [RelayCommand]
    private async Task OpenLogAsync(LogFile? logFile)
    {
        if (logFile == null) return;
        SelectedLogName = logFile.Name;
        await LoadLogContentAsync(logFile.FullPath);
    }

    private async Task LoadLogContentAsync(string fullPath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(fullPath);

            if (content.Length > 50000)
                content = content[^50000..] + "\n\n--- ARQUIVO TRUNCADO (últimos 50000 caracteres) ---";

            LogContent = content;
            StatusMessage = $"Visualizando: {Path.GetFileName(fullPath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao ler log: {ex.Message}";
        }
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

    partial void OnAutoRefreshChanged(bool value)
    {
        StatusMessage = value ? "Auto-refresh ativado" : "Auto-refresh desativado";
    }
}