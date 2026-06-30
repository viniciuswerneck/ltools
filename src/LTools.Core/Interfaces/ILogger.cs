namespace LTools.Core.Interfaces;

/// <summary>Severity level for log entries.</summary>
public enum LogLevel
{
    /// <summary>Detailed diagnostic information for development.</summary>
    Debug,
    /// <summary>General informational messages about application progress.</summary>
    Info,
    /// <summary>Potentially harmful situations that are not errors.</summary>
    Warning,
    /// <summary>Error events that might still allow the application to continue.</summary>
    Error
}

/// <summary>Logging abstraction used throughout the application.</summary>
public interface ILogger
{
    /// <summary>Writes a log entry at the specified level.</summary>
    void Log(LogLevel level, string message, Exception? exception = null);
    /// <summary>Writes a Debug-level log entry.</summary>
    void Debug(string message, Exception? exception = null) => Log(LogLevel.Debug, message, exception);
    /// <summary>Writes an Info-level log entry.</summary>
    void Info(string message, Exception? exception = null) => Log(LogLevel.Info, message, exception);
    /// <summary>Writes a Warning-level log entry.</summary>
    void Warning(string message, Exception? ex = null) => Log(LogLevel.Warning, message, ex);
    /// <summary>Writes an Error-level log entry.</summary>
    void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
}