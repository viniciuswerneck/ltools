using System.Collections.ObjectModel;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
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
    private string _healthColor = "#888888";

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private bool _isRunning;

    public bool NotRunning => !IsRunning;

    [ObservableProperty]
    private string _currentCheckName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private int _passedCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private int _warningCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private int _failedCount;

    public bool HasResults => PassedCount + WarningCount + FailedCount > 0;

    public ObservableCollection<DoctorCheck> Checks { get; } = [];
    public ObservableCollection<DoctorGroup> CheckGroups { get; } = [];

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
            _ = RunDiagnosticsAsync();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _projectPath = string.Empty;
            Checks.Clear();
            CheckGroups.Clear();
            HealthScore = 0;
            HealthLabel = "";
            HealthColor = "#888888";
            StatusMessage = "Selecione um projeto na barra superior.";
            InitFromContext();
        });
    }

    private List<DoctorCheck> BuildChecksList()
    {
        return
        [
            new DoctorCheck { Name = ".env", Category = "Arquivos Essenciais", WhatItChecks = "Arquivo de configuração de ambiente", Severity = CheckSeverity.Critical, FixCommand = "php -r copy('.env.example','.env');" },
            new DoctorCheck { Name = ".env.example", Category = "Arquivos Essenciais", WhatItChecks = "Modelo do arquivo de ambiente", Severity = CheckSeverity.Info },
            new DoctorCheck { Name = "composer.json", Category = "Arquivos Essenciais", WhatItChecks = "Definição de dependências do PHP", Severity = CheckSeverity.Critical },
            new DoctorCheck { Name = "package.json", Category = "Arquivos Essenciais", WhatItChecks = "Definição de dependências do Node", Severity = CheckSeverity.Info },
            new DoctorCheck { Name = "Dockerfile", Category = "Arquivos Essenciais", WhatItChecks = "Imagem personalizada do container", Severity = CheckSeverity.Info },
            new DoctorCheck { Name = "docker-compose.yml", Category = "Arquivos Essenciais", WhatItChecks = "Orquestração de containers", Severity = CheckSeverity.Info },
            new DoctorCheck { Name = ".gitignore", Category = "Arquivos Essenciais", WhatItChecks = "Arquivos ignorados pelo Git", Severity = CheckSeverity.Critical },

            new DoctorCheck { Name = "APP_KEY", Category = "Segurança", WhatItChecks = "Chave de criptografia da aplicação", Severity = CheckSeverity.Critical, FixCommand = "php artisan key:generate" },
            new DoctorCheck { Name = "APP_DEBUG", Category = "Segurança", WhatItChecks = "Modo debug desligado em produção", Severity = CheckSeverity.Critical },
            new DoctorCheck { Name = "APP_ENV", Category = "Segurança", WhatItChecks = "Ambiente configurado (local/production)", Severity = CheckSeverity.Warning },
            new DoctorCheck { Name = ".env no .gitignore", Category = "Segurança", WhatItChecks = ".env não está versionado no Git", Severity = CheckSeverity.Critical },

            new DoctorCheck { Name = "Storage Link", Category = "Estrutura", WhatItChecks = "Link simbólico public/storage \u2192 storage/app/public", Severity = CheckSeverity.Warning, FixCommand = "php artisan storage:link" },
            new DoctorCheck { Name = "Migrations", Category = "Estrutura", WhatItChecks = "Todas as migrations foram executadas", Severity = CheckSeverity.Critical, FixCommand = "php artisan migrate" },
            new DoctorCheck { Name = "Laravel Version", Category = "Estrutura", WhatItChecks = "Versão do Laravel instalada vs última disponível", Severity = CheckSeverity.Warning, FixCommand = "composer update laravel/framework --with-dependencies" },
            new DoctorCheck { Name = "Routes", Category = "Estrutura", WhatItChecks = "Arquivos de rota definidos (web, api)", Severity = CheckSeverity.Warning },

            new DoctorCheck { Name = "DB_CONNECTION", Category = "Banco de Dados", WhatItChecks = "Conexão com banco configurada no .env", Severity = CheckSeverity.Critical },
            new DoctorCheck { Name = "DB_HOST", Category = "Banco de Dados", WhatItChecks = "Host do banco configurado", Severity = CheckSeverity.Warning },
            new DoctorCheck { Name = "DB_DATABASE", Category = "Banco de Dados", WhatItChecks = "Nome do banco configurado", Severity = CheckSeverity.Warning },

            new DoctorCheck { Name = "CACHE_STORE", Category = "Cache e Sessão", WhatItChecks = "Driver de cache configurado", Severity = CheckSeverity.Warning },
            new DoctorCheck { Name = "SESSION_DRIVER", Category = "Cache e Sessão", WhatItChecks = "Driver de sessão configurado", Severity = CheckSeverity.Warning },
            new DoctorCheck { Name = "Config Cache", Category = "Cache e Sessão", WhatItChecks = "Cache de config otimizado (production)", Severity = CheckSeverity.Info, FixCommand = "php artisan config:cache" },
            new DoctorCheck { Name = "Route Cache", Category = "Cache e Sessão", WhatItChecks = "Cache de rotas otimizado (production)", Severity = CheckSeverity.Info, FixCommand = "php artisan route:cache" },

            new DoctorCheck { Name = "MAIL_MAILER", Category = "Email", WhatItChecks = "Driver de email configurado", Severity = CheckSeverity.Warning },
            new DoctorCheck { Name = "MAIL_FROM_ADDRESS", Category = "Email", WhatItChecks = "Remetente padrão configurado", Severity = CheckSeverity.Info },

            new DoctorCheck { Name = "QUEUE_CONNECTION", Category = "Fila", WhatItChecks = "Conexão de fila configurada", Severity = CheckSeverity.Info },

            new DoctorCheck { Name = "PHP", Category = "Ferramentas", WhatItChecks = "PHP instalado e acessível", Severity = CheckSeverity.Critical },
            new DoctorCheck { Name = "Composer", Category = "Ferramentas", WhatItChecks = "Composer instalado e acessível", Severity = CheckSeverity.Critical },
            new DoctorCheck { Name = "Git", Category = "Ferramentas", WhatItChecks = "Git instalado e acessível", Severity = CheckSeverity.Warning },
            new DoctorCheck { Name = "Node.js", Category = "Ferramentas", WhatItChecks = "Node.js instalado e acessível", Severity = CheckSeverity.Info },
            new DoctorCheck { Name = "NPM", Category = "Ferramentas", WhatItChecks = "NPM instalado e acessível", Severity = CheckSeverity.Info },
            new DoctorCheck { Name = "Pacotes Composer", Category = "Ferramentas", WhatItChecks = "Pacotes Composer desatualizados", Severity = CheckSeverity.Warning, FixCommand = "composer update" },
            new DoctorCheck { Name = "Pacotes NPM", Category = "Ferramentas", WhatItChecks = "Pacotes NPM desatualizados", Severity = CheckSeverity.Warning, FixCommand = "npm update" },

            new DoctorCheck { Name = "Laravel Sail", Category = "Docker", WhatItChecks = "laravel/sail instalado no composer.json", Severity = CheckSeverity.Info, FixCommand = "composer require laravel/sail --dev" },
            new DoctorCheck { Name = "Vendor", Category = "Docker", WhatItChecks = "Dependências PHP instaladas (vendor/)", Severity = CheckSeverity.Critical, FixCommand = "composer install" },
        ];
    }

    private void DefineChecks()
    {
        Checks.Clear();
        CheckGroups.Clear();
        foreach (var check in BuildChecksList())
            Checks.Add(check);
    }

    private void RebuildGroups()
    {
        CheckGroups.Clear();
        foreach (var group in Checks.GroupBy(c => c.Category))
        {
            CheckGroups.Add(new DoctorGroup
            {
                Category = group.Key,
                Items = new ObservableCollection<DoctorCheck>(group)
            });
        }
    }

    [RelayCommand]
    private async Task RunDiagnosticsAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        IsRunning = true;
        StatusMessage = "Analisando projeto...";

        DefineChecks();

        CurrentCheckName = "Arquivos essenciais";
        CheckEssentialFiles();

        CurrentCheckName = "Segurança";
        CheckSecurity();

        CurrentCheckName = "Banco de Dados";
        CheckDatabase();

        CurrentCheckName = "Cache e Sessão";
        CheckCache();

        CurrentCheckName = "Email";
        CheckMail();

        CurrentCheckName = "Fila";
        CheckQueue();

        CurrentCheckName = "Estrutura do projeto";
        await CheckStructureAsync();

        CurrentCheckName = "Ferramentas";
        CheckTools();

        CurrentCheckName = "Docker";
        CheckDocker();

        CurrentCheckName = "Versão do Laravel";
        await CheckLaravelVersionAsync();

        CurrentCheckName = "Pacotes desatualizados";
        await CheckOutdatedComposer();
        await CheckOutdatedNpm();

        CalculateHealthScore();

        StatusMessage = $"Diagnóstico: {PassedCount} OK" +
            (FailedCount > 0 ? $", {FailedCount} falha(s)" : "") +
            (WarningCount > 0 ? $", {WarningCount} alerta(s)" : "");

        CurrentCheckName = "";
        RebuildGroups();
        IsRunning = false;
    }

    private void CalculateHealthScore()
    {
        PassedCount = Checks.Count(c => c.Status == CheckStatus.Passed);
        WarningCount = Checks.Count(c => c.Status == CheckStatus.Warning);
        FailedCount = Checks.Count(c => c.Status == CheckStatus.Failed);

        var totalWeight = Checks.Sum(c => c.Weight);
        var earnedWeight = Checks.Sum(c => c.Status switch
        {
            CheckStatus.Passed => c.Weight,
            CheckStatus.Warning => c.Weight * 0.5,
            _ => 0
        });

        HealthScore = totalWeight > 0 ? (int)(earnedWeight / totalWeight * 100) : 0;

        (HealthLabel, HealthColor) = HealthScore switch
        {
            >= 90 => ("Excelente", "#4CAF50"),
            >= 70 => ("Bom", "#8BC34A"),
            >= 50 => ("Regular", "#FF9800"),
            _ => ("Crítico", "#F44336")
        };
    }

    [RelayCommand]
    private async Task FixAllAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        var toFix = Checks
            .Where(c => c.ShowFixButton && c.IsSafeFix)
            .ToList();

        if (toFix.Count == 0)
        {
            StatusMessage = "Nenhum item pendente de correção segura.";
            return;
        }

        IsRunning = true;
        var fixed_count = 0;
        var failed_count = 0;

        foreach (var check in toFix)
        {
            StatusMessage = $"Corrigindo: {check.Name}...";
            CurrentCheckName = check.Name;
            check.IsFixing = true;

            try
            {
                var parts = check.FixCommand!.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0];
                var args = parts.Length > 1 ? parts[1] : "";

                await _runner.RunAndGetOutputAsync(_projectPath, cmd, args);
                fixed_count++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Falha ao corrigir {check.Name}: {ex.Message}");
                failed_count++;
            }
        }

        await RunDiagnosticsAsync();
        StatusMessage = $"Correção em massa: {fixed_count} corrigido(s)" +
            (failed_count > 0 ? $", {failed_count} falha(s)" : "") +
            $". Diagnóstico re-executado.";
    }

    private async Task CheckOutdatedComposer()
    {
        var check = Checks.FirstOrDefault(c => c.Name == "Pacotes Composer");
        if (check == null) return;

        if (!File.Exists(Path.Combine(_projectPath, "composer.json")))
        {
            check.Status = CheckStatus.Passed;
            check.Message = "Sem dependências Composer";
            return;
        }

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "composer", "outdated --direct --no-ansi");
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var dataLines = lines.Count(l => l.Contains(" "));
            check.Status = dataLines > 0 ? CheckStatus.Warning : CheckStatus.Passed;
            check.Message = dataLines > 0 ? $"{dataLines} pacote(s) desatualizado(s)" : "Todos atualizados";
        }
        catch
        {
            check.Status = CheckStatus.Passed;
            check.Message = "Composer não instalado";
        }
    }

    private async Task CheckLaravelVersionAsync()
    {
        var check = Checks.FirstOrDefault(c => c.Name == "Laravel Version");
        if (check == null) return;

        try
        {
            var versionOutput = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan --version");
            var currentVersion = versionOutput.Split('\n').FirstOrDefault()?.Trim() ?? "desconhecida";

            try
            {
                var latestOutput = await _runner.RunAndGetOutputAsync(_projectPath, "composer", "show laravel/framework --latest --no-ansi");
                var latestMatch = System.Text.RegularExpressions.Regex.Match(latestOutput, @"latest\s*:\s*([^\s,]+)");

                if (latestMatch.Success)
                {
                    var latestVersion = latestMatch.Groups[1].Value;
                    var needsUpdate = !currentVersion.Contains(latestVersion, StringComparison.OrdinalIgnoreCase)
                        && !latestOutput.Contains("You are using the latest");

                    check.Status = needsUpdate ? CheckStatus.Warning : CheckStatus.Passed;
                    check.Message = needsUpdate
                        ? $"{currentVersion} → {latestVersion} disponível"
                        : $"{currentVersion} (última)";
                }
                else
                {
                    check.Status = CheckStatus.Passed;
                    check.Message = currentVersion;
                }
            }
            catch
            {
                check.Status = CheckStatus.Passed;
                check.Message = currentVersion;
            }
        }
        catch
        {
            check.Status = CheckStatus.Warning;
            check.Message = "Não foi possível detectar";
        }
    }

    private async Task CheckOutdatedNpm()
    {
        var check = Checks.FirstOrDefault(c => c.Name == "Pacotes NPM");
        if (check == null) return;

        if (!File.Exists(Path.Combine(_projectPath, "package.json")))
        {
            check.Status = CheckStatus.Passed;
            check.Message = "Sem dependências NPM";
            return;
        }

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "npm", "outdated --no-ansi");
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            check.Status = lines.Length > 1 ? CheckStatus.Warning : CheckStatus.Passed;
            check.Message = lines.Length > 1 ? $"{lines.Length - 1} pacote(s) desatualizado(s)" : "Todos atualizados";
        }
        catch
        {
            check.Status = CheckStatus.Passed;
            check.Message = "NPM não instalado";
        }
    }

    [RelayCommand]
    private async Task FixCheck(DoctorCheck? check)
    {
        if (check == null || string.IsNullOrWhiteSpace(check.FixCommand) || check.IsFixing) return;

        check.IsFixing = true;
        StatusMessage = $"Corrigindo: {check.Name}...";

        try
        {
            var parts = check.FixCommand.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0];
            var args = parts.Length > 1 ? parts[1] : "";

            await _runner.RunAndGetOutputAsync(_projectPath, cmd, args);
            await RunDiagnosticsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Falha ao corrigir {check.Name}: {ex.Message}";
            await RunDiagnosticsAsync();
        }
    }

    private void SetCheck(string name, CheckStatus status, string message, string? suggestion = null)
    {
        var check = Checks.FirstOrDefault(c => c.Name == name);
        if (check == null) return;
        check.Status = status;
        check.Message = message;
        if (suggestion != null) check.Suggestion = suggestion;
    }

    private void CheckEssentialFiles()
    {
        SetCheck(".env", File.Exists(Path.Combine(_projectPath, ".env")) ? CheckStatus.Passed : CheckStatus.Failed,
            File.Exists(Path.Combine(_projectPath, ".env")) ? ".env encontrado" : ".env não encontrado",
            "Copie .env.example para .env e configure");

        SetCheck(".env.example", File.Exists(Path.Combine(_projectPath, ".env.example")) ? CheckStatus.Passed : CheckStatus.Failed,
            File.Exists(Path.Combine(_projectPath, ".env.example")) ? ".env.example encontrado" : ".env.example não encontrado",
            "Crie um .env.example a partir do .env (sem senhas)");

        SetCheck("composer.json", File.Exists(Path.Combine(_projectPath, "composer.json")) ? CheckStatus.Passed : CheckStatus.Failed,
            File.Exists(Path.Combine(_projectPath, "composer.json")) ? "composer.json encontrado" : "composer.json não encontrado",
            "Execute: composer init");

        SetCheck("package.json", File.Exists(Path.Combine(_projectPath, "package.json")) ? CheckStatus.Passed : CheckStatus.Warning,
            File.Exists(Path.Combine(_projectPath, "package.json")) ? "package.json encontrado" : "package.json não encontrado",
            "Execute: npm init");

        SetCheck("Dockerfile", File.Exists(Path.Combine(_projectPath, "Dockerfile")) ? CheckStatus.Passed : CheckStatus.Warning,
            File.Exists(Path.Combine(_projectPath, "Dockerfile")) ? "Dockerfile encontrado" : "Dockerfile não encontrado",
            "Crie um Dockerfile ou use php:8.2-apache como base");

        var dc = File.Exists(Path.Combine(_projectPath, "docker-compose.yml"))
            || File.Exists(Path.Combine(_projectPath, "docker-compose.yaml"));
        SetCheck("docker-compose.yml", dc ? CheckStatus.Passed : CheckStatus.Warning,
            dc ? "docker-compose encontrado" : "docker-compose não encontrado",
            "Use Laravel Sail: php artisan sail:install");

        SetCheck(".gitignore", File.Exists(Path.Combine(_projectPath, ".gitignore")) ? CheckStatus.Passed : CheckStatus.Failed,
            File.Exists(Path.Combine(_projectPath, ".gitignore")) ? ".gitignore encontrado" : ".gitignore não encontrado",
            "Crie um .gitignore (gitignore.io pode ajudar)");
    }

    private void CheckSecurity()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath))
        {
            SetCheck("APP_KEY", CheckStatus.Failed, ".env não encontrado");
            SetCheck("APP_DEBUG", CheckStatus.Failed, ".env não encontrado");
            SetCheck("APP_ENV", CheckStatus.Failed, ".env não encontrado");
            SetCheck(".env no .gitignore", CheckStatus.Failed, ".env não encontrado");
            return;
        }

        var env = File.ReadAllText(envPath);

        var hasKey = env.Contains("APP_KEY=") && env.Contains("APP_KEY=base64:");
        SetCheck("APP_KEY", hasKey ? CheckStatus.Passed : CheckStatus.Failed,
            hasKey ? "APP_KEY configurada" : "APP_KEY inválida ou ausente",
            "Execute: php artisan key:generate");

        var isProd = env.Contains("APP_ENV=production");
        var debugOff = env.Contains("APP_DEBUG=false");
        var debugOk = !isProd || debugOff;
        SetCheck("APP_DEBUG", debugOk ? CheckStatus.Passed : CheckStatus.Failed,
            isProd && !debugOff ? "APP_DEBUG=true em produção!" : debugOff ? "APP_DEBUG=false" : "APP_DEBUG=true (ambiente local)",
            isProd ? "Defina APP_DEBUG=false em produção" : null);

        var envType = "";
        if (env.Contains("APP_ENV=production")) envType = "production";
        else if (env.Contains("APP_ENV=local")) envType = "local";
        else envType = "outro";
        SetCheck("APP_ENV", envType == "production" || envType == "local" ? CheckStatus.Passed : CheckStatus.Warning,
            $"APP_ENV={envType}",
            envType == "outro" ? "Defina APP_ENV=local ou production" : null);

        CheckEnvInGitIgnore();
    }

    private void CheckEnvInGitIgnore()
    {
        var gitignorePath = Path.Combine(_projectPath, ".gitignore");
        if (!File.Exists(gitignorePath))
        {
            SetCheck(".env no .gitignore", CheckStatus.Failed,
                ".gitignore não encontrado",
                "Crie um .gitignore com .env listado");
            return;
        }

        var gitignore = File.ReadAllText(gitignorePath);
        var hasEnv = gitignore.Contains(".env") && !gitignore.Contains(".env.example");
        SetCheck(".env no .gitignore", hasEnv ? CheckStatus.Passed : CheckStatus.Failed,
            hasEnv ? ".env está no .gitignore" : ".env NÃO está no .gitignore!",
            "Adicione .env ao .gitignore para evitar vazar credenciais");
    }

    private async Task CheckStructureAsync()
    {
        var link = Path.Combine(_projectPath, "public", "storage");
        var hasLink = Directory.Exists(link);
        SetCheck("Storage Link", hasLink ? CheckStatus.Passed : CheckStatus.Failed,
            hasLink ? "Link simbólico existe" : "Link simbólico não encontrado",
            "Execute: php artisan storage:link");

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan migrate:status --no-ansi");
            var pending = output.Contains("No migrations found") || output.Contains("Pending") || output.Contains("Nenhuma");
            SetCheck("Migrations", !pending ? CheckStatus.Passed : CheckStatus.Failed,
                pending ? "Migrations pendentes" : "Todas as migrations executadas",
                pending ? "Execute: php artisan migrate" : null);
        }
        catch
        {
            SetCheck("Migrations", CheckStatus.Failed, "Não foi possível verificar", "Verifique a conexão com o banco");
        }

        var routesDir = Path.Combine(_projectPath, "routes");
        var hasRoutes = Directory.Exists(routesDir) && Directory.GetFiles(routesDir, "*.php").Length > 0;
        SetCheck("Routes", hasRoutes ? CheckStatus.Passed : CheckStatus.Failed,
            hasRoutes ? "Rotas definidas" : "Nenhum arquivo de rota encontrado",
            "Crie routes/web.php e routes/api.php");
    }

    private void CheckDatabase()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath)) return;

        var env = File.ReadAllText(envPath);

        var dbConn = ExtractEnv(env, "DB_CONNECTION");
        SetCheck("DB_CONNECTION", !string.IsNullOrWhiteSpace(dbConn) ? CheckStatus.Passed : CheckStatus.Failed,
            dbConn != null ? $"DB_CONNECTION={dbConn}" : "DB_CONNECTION não configurado",
            "Adicione DB_CONNECTION=mysql no .env");

        var dbHost = ExtractEnv(env, "DB_HOST");
        SetCheck("DB_HOST", !string.IsNullOrWhiteSpace(dbHost) ? CheckStatus.Passed : CheckStatus.Failed,
            dbHost != null ? $"DB_HOST={dbHost}" : "DB_HOST não configurado",
            "Adicione DB_HOST=127.0.0.1 no .env");

        var dbName = ExtractEnv(env, "DB_DATABASE");
        SetCheck("DB_DATABASE", !string.IsNullOrWhiteSpace(dbName) ? CheckStatus.Passed : CheckStatus.Failed,
            dbName != null ? $"DB_DATABASE={dbName}" : "DB_DATABASE não configurado",
            "Adicione DB_DATABASE=nome_do_banco no .env");
    }

    private void CheckCache()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath)) return;

        var env = File.ReadAllText(envPath);

        var cache = ExtractEnv(env, "CACHE_STORE") ?? ExtractEnv(env, "CACHE_DRIVER");
        SetCheck("CACHE_STORE", !string.IsNullOrWhiteSpace(cache) ? CheckStatus.Passed : CheckStatus.Failed,
            cache != null ? $"CACHE_STORE={cache}" : "CACHE_STORE não configurado",
            "Adicione CACHE_STORE=file ou redis no .env");

        var session = ExtractEnv(env, "SESSION_DRIVER");
        SetCheck("SESSION_DRIVER", !string.IsNullOrWhiteSpace(session) ? CheckStatus.Passed : CheckStatus.Warning,
            session != null ? $"SESSION_DRIVER={session}" : "SESSION_DRIVER não configurado",
            "Adicione SESSION_DRIVER=file ou redis no .env");

        var configCached = File.Exists(Path.Combine(_projectPath, "bootstrap", "cache", "config.php"));
        SetCheck("Config Cache", configCached ? CheckStatus.Passed : CheckStatus.Warning,
            configCached ? "Cache de config ativo" : "Cache de config inativo",
            "Production: php artisan config:cache");

        var routeCached = File.Exists(Path.Combine(_projectPath, "bootstrap", "cache", "routes.php"))
            || File.Exists(Path.Combine(_projectPath, "bootstrap", "cache", "routes-v7.php"));
        SetCheck("Route Cache", routeCached ? CheckStatus.Passed : CheckStatus.Warning,
            routeCached ? "Cache de rotas ativo" : "Cache de rotas inativo",
            "Production: php artisan route:cache");
    }

    private void CheckMail()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath)) return;

        var env = File.ReadAllText(envPath);

        var mailer = ExtractEnv(env, "MAIL_MAILER");
        SetCheck("MAIL_MAILER", !string.IsNullOrWhiteSpace(mailer) ? CheckStatus.Passed : CheckStatus.Warning,
            mailer != null ? $"MAIL_MAILER={mailer}" : "MAIL_MAILER não configurado",
            "Adicione MAIL_MAILER=smtp ou log no .env");

        var fromAddr = ExtractEnv(env, "MAIL_FROM_ADDRESS");
        SetCheck("MAIL_FROM_ADDRESS", !string.IsNullOrWhiteSpace(fromAddr) ? CheckStatus.Passed : CheckStatus.Warning,
            fromAddr != null ? $"MAIL_FROM_ADDRESS={fromAddr}" : "MAIL_FROM_ADDRESS não configurado",
            "Adicione MAIL_FROM_ADDRESS=contato@exemplo.com no .env");
    }

    private void CheckQueue()
    {
        var envPath = Path.Combine(_projectPath, ".env");
        if (!File.Exists(envPath)) return;

        var env = File.ReadAllText(envPath);
        var queue = ExtractEnv(env, "QUEUE_CONNECTION");

        SetCheck("QUEUE_CONNECTION", !string.IsNullOrWhiteSpace(queue) ? CheckStatus.Passed : CheckStatus.Warning,
            queue != null ? $"QUEUE_CONNECTION={queue}" : "QUEUE_CONNECTION não configurado",
            "Adicione QUEUE_CONNECTION=database ou redis no .env");
    }

    private void CheckTools()
    {
        try
        {
            var php = RunVersion("php", "-v");
            SetCheck("PHP", !string.IsNullOrWhiteSpace(php) ? CheckStatus.Passed : CheckStatus.Failed,
                php ?? "PHP não instalado",
                "Instale o PHP 8.1+");
        }
        catch
        {
            SetCheck("PHP", CheckStatus.Failed, "PHP não instalado", "Instale o PHP 8.1+");
        }

        try
        {
            var comp = RunVersion("composer", "--version");
            SetCheck("Composer", !string.IsNullOrWhiteSpace(comp) ? CheckStatus.Passed : CheckStatus.Failed,
                comp ?? "Composer não instalado",
                "Instale o Composer");
        }
        catch
        {
            SetCheck("Composer", CheckStatus.Failed, "Composer não instalado", "Instale o Composer");
        }

        try
        {
            var git = RunVersion("git", "--version");
            SetCheck("Git", !string.IsNullOrWhiteSpace(git) ? CheckStatus.Passed : CheckStatus.Failed,
                git ?? "Git não instalado",
                "Instale o Git");
        }
        catch
        {
            SetCheck("Git", CheckStatus.Failed, "Git não instalado", "Instale o Git");
        }

        try
        {
            var node = RunVersion("node", "--version");
            SetCheck("Node.js", !string.IsNullOrWhiteSpace(node) ? CheckStatus.Passed : CheckStatus.Warning,
                node ?? "Node.js não instalado",
                "Instale o Node.js 18+");
        }
        catch
        {
            SetCheck("Node.js", CheckStatus.Warning, "Node.js não instalado", "Instale o Node.js 18+");
        }

        try
        {
            var npm = RunVersion("npm", "--version");
            SetCheck("NPM", !string.IsNullOrWhiteSpace(npm) ? CheckStatus.Passed : CheckStatus.Warning,
                npm ?? "NPM não instalado",
                "Instale o NPM");
        }
        catch
        {
            SetCheck("NPM", CheckStatus.Warning, "NPM não instalado", "Instale o NPM");
        }
    }

    private void CheckDocker()
    {
        var composerPath = Path.Combine(_projectPath, "composer.json");
        if (File.Exists(composerPath))
        {
            var json = File.ReadAllText(composerPath);
            var hasSail = json.Contains("\"laravel/sail\"");
            SetCheck("Laravel Sail", hasSail ? CheckStatus.Passed : CheckStatus.Warning,
                hasSail ? "Sail instalado no projeto" : "Sail não encontrado",
                "Execute: composer require laravel/sail --dev && php artisan sail:install");
        }

        var vendorExists = Directory.Exists(Path.Combine(_projectPath, "vendor"));
        SetCheck("Vendor", vendorExists ? CheckStatus.Passed : CheckStatus.Failed,
            vendorExists ? "vendor/ encontrado" : "vendor/ não encontrado",
            "Execute: composer install");
    }

    private static string? ExtractEnv(string envContent, string key)
    {
        foreach (var line in envContent.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith($"{key}="))
            {
                var val = trimmed[(key.Length + 1)..];
                return string.IsNullOrWhiteSpace(val) ? null : val;
            }
        }
        return null;
    }

    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(NotRunning));
    }

    private static string? RunVersion(string command, string args)
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
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output.Split('\n').FirstOrDefault()?.Trim();
    }

    [RelayCommand]
    private void ClearResults()
    {
        DefineChecks();
        RebuildGroups();
        HealthScore = 0;
        HealthLabel = "";
        HealthColor = "#888888";
        PassedCount = 0;
        WarningCount = 0;
        FailedCount = 0;
        CurrentCheckName = "";
        StatusMessage = "Resultados limpos. Execute o diagnóstico novamente.";
    }

    [RelayCommand]
    private async Task ExportTxtAsync()
    {
        try
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (window?.StorageProvider is null)
            {
                StatusMessage = "Erro ao acessar o seletor de arquivos.";
                return;
            }

            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Exportar relatório",
                DefaultExtension = "txt",
                FileTypeChoices = [new FilePickerFileType("Texto") { Patterns = ["*.txt"] }],
                SuggestedFileName = $"diagnostico_{ProjectName}_{DateTime.Now:yyyyMMdd_HHmmss}"
            });

            if (file is null) return;

            var report = GenerateReport();
            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(report);

            StatusMessage = $"Relatório exportado: {file.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao exportar: {ex.Message}";
        }
    }

    private string GenerateReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("    RELATÓRIO DE DIAGNÓSTICO - DOCTOR");
        sb.AppendLine("========================================");
        sb.AppendLine();
        sb.AppendLine($"Projeto: {ProjectName}");
        sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine($"Saúde: {HealthScore}% - {HealthLabel}");
        sb.AppendLine();
        sb.AppendLine("Resumo:");
        sb.AppendLine($"  OK:     {PassedCount}");
        sb.AppendLine($"  Alerta: {WarningCount}");
        sb.AppendLine($"  Falha:  {FailedCount}");
        sb.AppendLine();
        sb.AppendLine("----------------------------------------");

        foreach (var group in CheckGroups)
        {
            sb.AppendLine();
            sb.AppendLine($"  [{group.Category}]");
            sb.AppendLine("----------------------------------------");

            foreach (var check in group.Items)
            {
                sb.AppendLine($"  {check.Icon} {check.Name}");
                sb.AppendLine($"     O que verifica: {check.WhatItChecks}");
                sb.AppendLine($"     Status: {check.Message}");
                if (!string.IsNullOrWhiteSpace(check.Suggestion))
                    sb.AppendLine($"     Sugestão: {check.Suggestion}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("========================================");
        sb.AppendLine("  Gerado por LTools - Laravel Toolkit");
        sb.AppendLine("========================================");

        return sb.ToString();
    }
}
