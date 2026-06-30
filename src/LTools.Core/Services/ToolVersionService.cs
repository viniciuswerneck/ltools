using System.Diagnostics;
using LTools.Core.Interfaces;

namespace LTools.Core.Services;

public class ToolVersionService : IToolVersionService
{
    private readonly ILogger _logger;

    public ToolVersionService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<string> GetVersionAsync(string command, string arguments = "--version")
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output.Split('\n').FirstOrDefault()?.Trim() ?? "Não detectado";
        }
        catch (Exception ex)
        {
            _logger.Debug($"Ferramenta não encontrada: {command} — {ex.Message}");
            return "Não instalado";
        }
    }

    public Task<string> GetPhpVersionAsync() => GetVersionAsync("php", "-v");
    public Task<string> GetComposerVersionAsync() => GetVersionAsync("composer", "--version");
    public Task<string> GetGitVersionAsync() => GetVersionAsync("git", "--version");
    public Task<string> GetNodeVersionAsync() => GetVersionAsync("node", "--version");
    public Task<string> GetNpmVersionAsync() => GetVersionAsync("npm", "--version");
    public Task<string> GetDockerVersionAsync() => GetVersionAsync("docker", "--version");
    public Task<string> GetLaravelCliVersionAsync() => GetVersionAsync("laravel", "--version");
}