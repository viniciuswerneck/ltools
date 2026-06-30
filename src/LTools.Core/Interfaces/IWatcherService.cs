namespace LTools.Core.Interfaces;

/// <summary>Continuously monitors files and processes (logs, queue, tests).</summary>
public interface IWatcherService
{
    /// <summary>Raised when a watched file changes.</summary>
    event Action<string>? FileChanged;
    /// <summary>Raised when new log output is detected.</summary>
    event Action<string>? LogOutput;
    /// <summary>Whether the watcher is currently running.</summary>
    bool IsWatching { get; }
    /// <summary>Starts watching the given log file path.</summary>
    void StartWatch(string logPath);
    /// <summary>Stops watching.</summary>
    void StopWatch();
}
