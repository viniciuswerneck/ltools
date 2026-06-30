namespace LTools.Core.Interfaces;

/// <summary>Abstraction over System.Diagnostics.Process for running shell commands.</summary>
public interface IProcessRunner
{
    /// <summary>Raised when the process writes to stdout.</summary>
    event Action<string>? OutputReceived;
    /// <summary>Raised when the process writes to stderr.</summary>
    event Action<string>? ErrorReceived;
    /// <summary>Raised when the process exits.</summary>
    event Action? ProcessExited;

    /// <summary>Starts a process and returns the exit code.</summary>
    Task<int> RunAsync(string workingDirectory, string command, string arguments);
    /// <summary>Starts a process and captures all output as a single string.</summary>
    Task<string> RunAndGetOutputAsync(string workingDirectory, string command, string arguments);
    /// <summary>Forcefully terminates the running process.</summary>
    void Kill();
}
