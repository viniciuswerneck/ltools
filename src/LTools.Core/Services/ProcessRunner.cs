using System.Diagnostics;
using System.Text;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ProcessRunner : IProcessRunner
{
    private Process? _process;

    public event Action<string>? OutputReceived;
    public event Action<string>? ErrorReceived;
    public event Action? ProcessExited;

    public async Task<int> RunAsync(string workingDirectory, string command, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        _process = process;

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                OutputReceived?.Invoke(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                ErrorReceived?.Invoke(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        // Drain async output buffers — recommended .NET pattern
        // WaitForExit() after WaitForExitAsync() ensures all async
        // OutputDataReceived/ErrorDataReceived events are delivered
        process.WaitForExit();

        ProcessExited?.Invoke();

        return process.ExitCode;
    }

    public async Task<string> RunAndGetOutputAsync(string workingDirectory, string command, string arguments)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) output.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) error.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cts.Token);

        return output.ToString() + error.ToString();
    }

    public void Kill()
    {
        _process?.Kill(entireProcessTree: true);
        _process?.Dispose();
        _process = null;
    }
}
