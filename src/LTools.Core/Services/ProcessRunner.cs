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
        var tcs = new TaskCompletionSource<int>();

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

        process.Exited += (_, _) =>
        {
            ProcessExited?.Invoke();
            tcs.TrySetResult(process.ExitCode);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return await tcs.Task;
    }

    public async Task<string> RunAndGetOutputAsync(string workingDirectory, string command, string arguments)
    {
        var tcs = new TaskCompletionSource<string>();

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

        process.Exited += (_, _) =>
        {
            process.WaitForExit();
            tcs.TrySetResult(output.ToString() + error.ToString());
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var result = await tcs.Task;
        process.Dispose();
        return result;
    }

    public void Kill()
    {
        _process?.Kill(entireProcessTree: true);
        _process?.Dispose();
        _process = null;
    }
}
