using System.Diagnostics;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        var prefix = level switch
        {
            LogLevel.Debug => "DBG",
            LogLevel.Info => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            _ => "???"
        };

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var line = $"[{timestamp}] [{prefix}] {message}";
        if (exception != null)
            line += $" | {exception.GetType().Name}: {exception.Message}";

        Debug.WriteLine(line);
        Console.Error.WriteLine(line);
    }
}