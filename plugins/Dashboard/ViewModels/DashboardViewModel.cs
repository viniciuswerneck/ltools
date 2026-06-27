using CommunityToolkit.Mvvm.ComponentModel;

namespace LTools.Dashboard.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _phpVersion = "Carregando...";

    [ObservableProperty]
    private string _laravelVersion = "Carregando...";

    [ObservableProperty]
    private string _composerVersion = "Carregando...";

    [ObservableProperty]
    private string _gitVersion = "Carregando...";

    [ObservableProperty]
    private string _nodeVersion = "Carregando...";

    [ObservableProperty]
    private string _dockerVersion = "Carregando...";

    [ObservableProperty]
    private int _recentProjectsCount;

    public async Task LoadAsync()
    {
        PhpVersion = await GetVersionAsync("php", "-v");
        LaravelVersion = await GetGlobalVersionAsync("laravel", "--version");
        ComposerVersion = await GetVersionAsync("composer", "--version");
        GitVersion = await GetVersionAsync("git", "--version");
        NodeVersion = await GetVersionAsync("node", "--version");
        DockerVersion = await GetVersionAsync("docker", "--version");
    }

    private static async Task<string> GetVersionAsync(string command, string args)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = psi };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output.Split('\n').FirstOrDefault()?.Trim() ?? "Não detectado";
        }
        catch
        {
            return "Não instalado";
        }
    }

    private static async Task<string> GetGlobalVersionAsync(string package, string args)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "php",
                Arguments = $"artisan {package} {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = psi };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output.Split('\n').FirstOrDefault()?.Trim() ?? "Não detectado";
        }
        catch
        {
            return "Não instalado";
        }
    }
}
