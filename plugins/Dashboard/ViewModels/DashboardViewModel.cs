using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Interfaces;
using LTools.Core.Services;

namespace LTools.Dashboard.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IToolVersionService _toolVersion;
    private readonly ILogger _logger;
    private readonly IProjectProfileService? _profile;

    [ObservableProperty]
    private string _phpVersion = "Carregando...";

    [ObservableProperty]
    private string _laravelVersion = "Carregando...";

    [ObservableProperty]
    private string _laravelCliVersion = "Carregando...";

    [ObservableProperty]
    private string _composerVersion = "Carregando...";

    [ObservableProperty]
    private string _gitVersion = "Carregando...";

    [ObservableProperty]
    private string _nodeVersion = "Carregando...";

    [ObservableProperty]
    private string _dockerVersion = "Carregando...";

    [ObservableProperty]
    private string _statusMessage = "Nenhum projeto aberto.";

    [ObservableProperty]
    private bool _hasProject;

    [ObservableProperty]
    private string _projectName = "";

    [ObservableProperty]
    private string _projectPath = "";

    [ObservableProperty]
    private string _projectSize = "";

    [ObservableProperty]
    private string _env = "";

    [ObservableProperty]
    private string _debugMode = "";

    [ObservableProperty]
    private string _url = "";

    [ObservableProperty]
    private string _broadcasting = "";

    [ObservableProperty]
    private string _cacheDriver = "";

    [ObservableProperty]
    private string _databaseDriver = "";

    [ObservableProperty]
    private string _logsDriver = "";

    [ObservableProperty]
    private string _cacheConfig = "";

    [ObservableProperty]
    private string _cacheEvents = "";

    [ObservableProperty]
    private string _cacheRoutes = "";

    [ObservableProperty]
    private string _cacheViews = "";

    [ObservableProperty]
    private int _migrationsCount;

    [ObservableProperty]
    private int _modelsCount;

    [ObservableProperty]
    private int _controllersCount;

    [ObservableProperty]
    private int _jobsCount;

    [ObservableProperty]
    private int _servicesCount;

    [ObservableProperty]
    private int _middlewareCount;

    [ObservableProperty]
    private int _eventsCount;

    [ObservableProperty]
    private int _listenersCount;

    [ObservableProperty]
    private int _notificationsCount;

    [ObservableProperty]
    private int _mailCount;

    [ObservableProperty]
    private int _observersCount;

    [ObservableProperty]
    private int _policiesCount;

    [ObservableProperty]
    private int _providersCount;

    [ObservableProperty]
    private int _commandsCount;

    [ObservableProperty]
    private int _factoriesCount;

    [ObservableProperty]
    private int _seedersCount;

    [ObservableProperty]
    private int _testsCount;

    [ObservableProperty]
    private int _routesCount;

    [ObservableProperty]
    private int _packagesCount;

    [ObservableProperty]
    private int _devPackagesCount;

    public DashboardViewModel()
    {
        _toolVersion = AppServices.Get<IToolVersionService>() ?? new ToolVersionService(AppServices.Get<ILogger>() ?? new ConsoleLogger());
        _logger = AppServices.Get<ILogger>() ?? new ConsoleLogger();
        _profile = AppServices.Get<IProjectProfileService>();
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        _ = LoadAsync();
        InitFromContext();
    }

    private void InitFromContext()
    {
        var path = ProjectContext.Instance.CurrentPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            ProjectPath = path;
            ProjectName = ProjectContext.Instance.CurrentName ?? "";
            HasProject = true;
            StatusMessage = $"Projeto: {ProjectName}";
            _ = LoadProjectInfoAsync();
        }
        else
        {
            HasProject = false;
            StatusMessage = "Nenhum projeto aberto. Selecione um na barra superior.";
        }
    }

    private void OnProjectChanged()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(InitFromContext);
    }

    public async Task LoadAsync()
    {
        PhpVersion = await _toolVersion.GetPhpVersionAsync();
        LaravelCliVersion = await _toolVersion.GetLaravelCliVersionAsync();
        ComposerVersion = await _toolVersion.GetComposerVersionAsync();
        GitVersion = await _toolVersion.GetGitVersionAsync();
        NodeVersion = await _toolVersion.GetNodeVersionAsync();
        DockerVersion = await _toolVersion.GetDockerVersionAsync();
    }

    private async Task LoadProjectInfoAsync()
    {
        if (string.IsNullOrWhiteSpace(ProjectPath)) return;

        LoadFileCounts();
        await LoadComposerInfoAsync();
        await LoadArtisanAboutAsync();
        LoadProjectSize();
    }

    private void LoadFileCounts()
    {
        MigrationsCount = CountFiles("database", "migrations", "*.php");
        ModelsCount = CountFiles("app", "Models", "*.php");
        ControllersCount = CountFiles("app", "Http/Controllers", "*.php");
        JobsCount = CountFiles("app", "Jobs", "*.php");
        ServicesCount = CountFiles("app", "Services", "*.php");
        MiddlewareCount = CountFiles("app", "Http/Middleware", "*.php");
        EventsCount = CountFiles("app", "Events", "*.php");
        ListenersCount = CountFiles("app", "Listeners", "*.php");
        NotificationsCount = CountFiles("app", "Notifications", "*.php");
        MailCount = CountFiles("app", "Mail", "*.php");
        ObserversCount = CountFiles("app", "Observers", "*.php");
        PoliciesCount = CountFiles("app", "Policies", "*.php");
        ProvidersCount = CountFiles("app", "Providers", "*.php");
        CommandsCount = CountFiles("app", "Console/Commands", "*.php");
        FactoriesCount = CountFiles("database", "factories", "*.php");
        SeedersCount = CountFiles("database", "seeders", "*.php");
        TestsCount = CountFiles("tests", null, "*.php");
        RoutesCount = CountRouteFiles();
    }

    private int CountFiles(string baseDir, string? subDir, string pattern)
    {
        var dir = subDir != null
            ? Path.Combine(ProjectPath, baseDir, subDir)
            : Path.Combine(ProjectPath, baseDir);
        if (!Directory.Exists(dir)) return 0;
        try { return Directory.GetFiles(dir, pattern).Length; }
        catch (Exception ex) { _logger.Debug($"Erro ao contar arquivos em {dir}: {ex.Message}"); return 0; }
    }

    private int CountRouteFiles()
    {
        var routesDir = Path.Combine(ProjectPath, "routes");
        if (!Directory.Exists(routesDir)) return 0;
        try { return Directory.GetFiles(routesDir, "*.php").Length; }
        catch (Exception ex) { _logger.Debug($"Erro ao contar rotas: {ex.Message}"); return 0; }
    }

    private async Task LoadComposerInfoAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(Path.Combine(ProjectPath, "composer.json"));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("require", out var require))
                PackagesCount = require.EnumerateObject().Count();

            if (root.TryGetProperty("require-dev", out var dev))
                DevPackagesCount = dev.EnumerateObject().Count();
        }
        catch (Exception ex)
        {
            _logger.Warning("Erro ao ler composer.json", ex);
        }
    }

    private async Task LoadArtisanAboutAsync()
    {
        var runner = AppServices.Get<IProcessRunner>() ?? new ProcessRunner();

        try
        {
            var output = await runner.RunAndGetOutputAsync(ProjectPath, "php", "artisan about --json --no-ansi");

            if (!string.IsNullOrWhiteSpace(output))
            {
                using var doc = JsonDocument.Parse(output);
                var root = doc.RootElement;

                if (root.TryGetProperty("environment", out var env))
                {
                    LaravelVersion = TryGetString(env, "application_version");
                    PhpVersion = TryGetString(env, "php_version");
                    Env = TryGetString(env, "environment");
                    DebugMode = TryGetString(env, "debug_mode");
                    Url = TryGetString(env, "url");
                }

                if (root.TryGetProperty("cache", out var cache))
                {
                    CacheConfig = TryGetString(cache, "config");
                    CacheEvents = TryGetString(cache, "events");
                    CacheRoutes = TryGetString(cache, "routes");
                    CacheViews = TryGetString(cache, "views");
                }

                if (root.TryGetProperty("drivers", out var drivers))
                {
                    Broadcasting = TryGetString(drivers, "broadcasting");
                    CacheDriver = TryGetString(drivers, "cache");
                    DatabaseDriver = TryGetString(drivers, "database");
                    LogsDriver = TryGetString(drivers, "logs");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning("Erro ao executar php artisan about", ex);
        }

        if (string.IsNullOrWhiteSpace(LaravelVersion) || LaravelVersion == "Carregando...")
        {
            try
            {
                var version = await runner.RunAndGetOutputAsync(ProjectPath, "php", "artisan --version");
                if (!string.IsNullOrWhiteSpace(version))
                    LaravelVersion = version.Split('\n').FirstOrDefault()?.Trim() ?? "";
            }
            catch (Exception ex)
            {
                _logger.Debug("Erro ao obter versão do Laravel via artisan", ex);
            }
        }

        if (string.IsNullOrWhiteSpace(LaravelVersion) || LaravelVersion == "Carregando...")
            LoadLaravelVersionFromComposer();
    }

    private void LoadLaravelVersionFromComposer()
    {
        try
        {
            var json = File.ReadAllText(Path.Combine(ProjectPath, "composer.json"));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("require", out var require) &&
                require.TryGetProperty("laravel/framework", out var fw))
            {
                LaravelVersion = fw.GetString() ?? "";
            }
        }
        catch (Exception ex)
        {
            _logger.Debug("Erro ao ler versão Laravel do composer.json", ex);
        }
    }

    private static string TryGetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var prop) ? prop.GetString() ?? "" : "";
    }

    private void LoadProjectSize()
    {
        try
        {
            var dir = new DirectoryInfo(ProjectPath);
            if (!dir.Exists) { ProjectSize = "Desconhecido"; return; }

            long size = 0;
            foreach (var sub in dir.EnumerateDirectories())
            {
                if (sub.Name is ".git" or "node_modules" or "vendor" or ".idea" or ".vscode")
                    continue;
                size += DirSize(sub);
            }
            ProjectSize = FormatSize(size);
        }
        catch (Exception ex)
        {
            _logger.Debug("Erro ao calcular tamanho do projeto", ex);
            ProjectSize = "Desconhecido";
        }
    }

    private static long DirSize(DirectoryInfo d)
    {
        long size = 0;
        try
        {
            foreach (var f in d.EnumerateFiles("*", SearchOption.AllDirectories))
                size += f.Length;
        }
        catch
        {
            // skip inaccessible files
        }
        return size;
    }

    private static string FormatSize(long bytes)
    {
        string[] suf = ["B", "KB", "MB", "GB", "TB"];
        int place = 0;
        double sz = bytes;
        while (sz >= 1024 && place < suf.Length - 1) { sz /= 1024; place++; }
        return $"{sz:F2} {suf[place]}";
    }

    [RelayCommand]
    private void OpenForge()
    {
        try { Process.Start(new ProcessStartInfo("https://forge.laravel.com") { UseShellExecute = true }); }
        catch (Exception ex) { _logger.Warning("Falha ao abrir Forge", ex); }
    }

    [RelayCommand]
    private void OpenVapor()
    {
        try { Process.Start(new ProcessStartInfo("https://vapor.laravel.com") { UseShellExecute = true }); }
        catch (Exception ex) { _logger.Warning("Falha ao abrir Vapor", ex); }
    }

    [RelayCommand]
    private void OpenEnvoyer()
    {
        try { Process.Start(new ProcessStartInfo("https://envoyer.io") { UseShellExecute = true }); }
        catch (Exception ex) { _logger.Warning("Falha ao abrir Envoyer", ex); }
    }

    [RelayCommand]
    private void OpenTelescope()
    {
        if (string.IsNullOrWhiteSpace(ProjectPath)) return;
        try { Process.Start(new ProcessStartInfo("http://localhost/telescope") { UseShellExecute = true }); }
        catch (Exception ex) { _logger.Warning("Falha ao abrir Telescope", ex); }
    }
}