using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.ProjectDoctor.Models;

namespace LTools.ProjectDoctor.ViewModels;

public partial class ProjectDoctorViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private int _healthScore;

    [ObservableProperty]
    private string _healthLabel = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    public ObservableCollection<DoctorCheck> Checks { get; } = [];

    public ProjectDoctorViewModel()
    {
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        InitFromContext();
    }

    private void InitFromContext()
    {
        var path = ProjectContext.Instance.CurrentPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            _projectPath = path;
            ProjectName = ProjectContext.Instance.CurrentName ?? "";
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Checks.Clear();
            HealthScore = 0;
            HealthLabel = string.Empty;
            InitFromContext();
        });
    }

    [RelayCommand]
    private async Task RunDiagnosticsAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        Checks.Clear();
        StatusMessage = "Executando diagnósticos...";

        await CheckAppKeyAsync();
        CheckEnv();
        CheckStorageLink();
        await CheckDebugModeAsync();
        await CheckMigrationsAsync();
        await CheckComposerAsync();
        await CheckPhpVersionAsync();

        var passed = Checks.Count(c => c.Passed);
        HealthScore = (int)((double)passed / Checks.Count * 100);
        HealthLabel = HealthScore switch
        {
            >= 90 => "Excelente ✅",
            >= 70 => "Bom 👍",
            >= 50 => "Regular ⚠️",
            _ => "Crítico 🔴"
        };

        StatusMessage = $"Diagnóstico concluído: {passed}/{Checks.Count} verificações OK";
    }

    private async Task CheckAppKeyAsync()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath))
        {
            Checks.Add(new DoctorCheck { Name = "APP_KEY", Category = "Segurança", Passed = false, Message = ".env não encontrado", Suggestion = "Crie o arquivo .env" });
            return;
        }

        var env = await File.ReadAllTextAsync(envPath);
        var hasKey = env.Contains("APP_KEY=") && !env.Contains("APP_KEY=base64:");
        var passed = hasKey;
        Checks.Add(new DoctorCheck
        {
            Name = "APP_KEY",
            Category = "Segurança",
            Passed = passed,
            Message = passed ? "APP_KEY configurada" : "APP_KEY não configurada ou inválida",
            Suggestion = passed ? null : "Execute: php artisan key:generate"
        });
    }

    private async Task CheckDebugModeAsync()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath)) return;

        var env = await File.ReadAllTextAsync(envPath);
        var isProduction = env.Contains("APP_ENV=production") && env.Contains("APP_DEBUG=false");
        Checks.Add(new DoctorCheck
        {
            Name = "APP_DEBUG",
            Category = "Segurança",
            Passed = isProduction,
            Message = isProduction ? "DEBUG desligado em produção" : "DEBUG pode estar ligado",
            Suggestion = isProduction ? null : "Defina APP_DEBUG=false em produção"
        });
    }

    private void CheckEnv()
    {
        var exists = File.Exists(Path.Combine(_projectPath, ".env"));
        Checks.Add(new DoctorCheck
        {
            Name = ".env",
            Category = "Configuração",
            Passed = exists,
            Message = exists ? ".env encontrado" : ".env não encontrado",
            Suggestion = exists ? null : "Copie .env.example para .env"
        });
    }

    private void CheckStorageLink()
    {
        var link = Path.Combine(_projectPath, "public", "storage");
        var exists = Directory.Exists(link);
        Checks.Add(new DoctorCheck
        {
            Name = "Storage Link",
            Category = "Estrutura",
            Passed = exists,
            Message = exists ? "Link simbólico do storage existe" : "Link simbólico não encontrado",
            Suggestion = exists ? null : "Execute: php artisan storage:link"
        });
    }

    private async Task CheckMigrationsAsync()
    {
        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan migrate:status --no-ansi");
            var pending = output.Contains("No migrations found") || output.Contains("Pending");
            Checks.Add(new DoctorCheck
            {
                Name = "Migrations",
                Category = "Banco",
                Passed = !pending,
                Message = pending ? "Migrations pendentes" : "Todas as migrations foram executadas",
                Suggestion = pending ? "Execute: php artisan migrate" : null
            });
        }
        catch
        {
            Checks.Add(new DoctorCheck { Name = "Migrations", Category = "Banco", Passed = false, Message = "Não foi possível verificar migrations", Suggestion = "Verifique a conexão com o banco" });
        }
    }

    private async Task CheckComposerAsync()
    {
        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "composer", "--version");
            var hasComposer = !string.IsNullOrWhiteSpace(output);
            Checks.Add(new DoctorCheck
            {
                Name = "Composer",
                Category = "Ferramentas",
                Passed = hasComposer,
                Message = hasComposer ? "Composer instalado" : "Composer não encontrado",
                Suggestion = hasComposer ? null : "Instale o Composer"
            });
        }
        catch
        {
            Checks.Add(new DoctorCheck { Name = "Composer", Category = "Ferramentas", Passed = false, Message = "Composer não instalado", Suggestion = "Instale o Composer" });
        }
    }

    private async Task CheckPhpVersionAsync()
    {
        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "-v");
            var versionLine = output.Split('\n').FirstOrDefault()?.Trim() ?? "";
            var hasPhp = !string.IsNullOrWhiteSpace(versionLine);
            Checks.Add(new DoctorCheck
            {
                Name = "PHP",
                Category = "Ferramentas",
                Passed = hasPhp,
                Message = hasPhp ? versionLine : "PHP não encontrado",
                Suggestion = hasPhp ? null : "Instale o PHP"
            });
        }
        catch
        {
            Checks.Add(new DoctorCheck { Name = "PHP", Category = "Ferramentas", Passed = false, Message = "PHP não instalado", Suggestion = "Instale o PHP" });
        }
    }

    [RelayCommand]
    private void ClearResults()
    {
        Checks.Clear();
        HealthScore = 0;
        HealthLabel = "";
        StatusMessage = "Resultados limpos.";
    }
}