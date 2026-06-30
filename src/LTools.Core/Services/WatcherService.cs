using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class WatcherService : IWatcherService, IDisposable
{
    private readonly ILogger _logger;
    private FileSystemWatcher? _watcher;
    private long _lastFileSize;
    private string? _watchingPath;

    public event Action<string>? FileChanged;
    public event Action<string>? LogOutput;
    public bool IsWatching => _watcher != null;

    public WatcherService(ILogger logger)
    {
        _logger = logger;
    }

    public void StartWatch(string logPath)
    {
        StopWatch();

        var dir = Path.GetDirectoryName(logPath);
        var file = Path.GetFileName(logPath);

        if (!Directory.Exists(dir))
        {
            _logger.Warning($"Watcher: diretório não encontrado: {dir}");
            return;
        }

        _watchingPath = logPath;
        _lastFileSize = File.Exists(logPath) ? new FileInfo(logPath).Length : 0;

        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnChanged;
        _watcher.Error += (_, e) => _logger.Error("Watcher error", e.GetException());

        _logger.Info($"Watcher iniciado: {logPath}");
    }

    public void StopWatch()
    {
        if (_watcher == null) return;

        _watcher.Changed -= OnChanged;
        _watcher.Dispose();
        _watcher = null;
        _logger.Info("Watcher parado.");
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (!File.Exists(_watchingPath)) return;

            var fi = new FileInfo(_watchingPath);
            var newSize = fi.Length;

            if (newSize <= _lastFileSize) return;

            using var stream = File.Open(_watchingPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            stream.Seek(_lastFileSize, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            var newContent = reader.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(newContent))
            {
                var lines = newContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                    LogOutput?.Invoke(line.TrimEnd('\r'));
            }

            _lastFileSize = newSize;
            FileChanged?.Invoke(_watchingPath);
        }
        catch (Exception ex)
        {
            _logger.Debug($"Watcher read error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        StopWatch();
    }
}
