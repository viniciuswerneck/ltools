namespace LTools.Core.Interfaces;

public interface IProcessRunner
{
    event Action<string>? OutputReceived;
    event Action<string>? ErrorReceived;
    event Action? ProcessExited;

    Task<int> RunAsync(string workingDirectory, string command, string arguments);
    Task<string> RunAndGetOutputAsync(string workingDirectory, string command, string arguments);
    void Kill();
}
