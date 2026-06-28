using System.Diagnostics;
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
        var output = new StringWriter();

        void OutHandler(string data) => output.WriteLine(data);
        void ErrHandler(string data) => output.WriteLine(data);

        OutputReceived += OutHandler;
        ErrorReceived += ErrHandler;
        await RunAsync(workingDirectory, command, arguments);
        OutputReceived -= OutHandler;
        ErrorReceived -= ErrHandler;

        return output.ToString();
    }

    public void Kill()
    {
        _process?.Kill(entireProcessTree: true);
        _process?.Dispose();
        _process = null;
    }
}
