using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.ArtisanGui.Models;
using LTools.Core.Interfaces;
using LTools.Core.Services;

namespace LTools.ArtisanGui.ViewModels;

public partial class ArtisanGuiViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private readonly IConfigManager? _config;
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ArtisanCommand? _selectedCommand;

    [ObservableProperty]
    private string _customCommand = string.Empty;

    [ObservableProperty]
    private string _argumentValues = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _commandsLoaded;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private bool _dangerWarningVisible;

    [ObservableProperty]
    private string _dangerCommandName = string.Empty;

    public ObservableCollection<ArtisanCommand> AllCommands { get; } = [];
    public ObservableCollection<ArtisanCommand> FilteredCommands { get; } = [];
    public ObservableCollection<CommandHistory> History { get; } = [];

    private static readonly HashSet<string> ComandosPerigosos =
    [
        "migrate:fresh", "migrate:refresh", "migrate:reset",
        "db:wipe", "db:drop", "schema:drop"
    ];

    private static readonly Dictionary<string, string> Traducoes = new()
    {
        ["Display the current application status"] = "Exibe o status atual da aplicação",
        ["Display the current application environment"] = "Exibe o ambiente atual da aplicação",
        ["Display the current date"] = "Exibe a data atual",
        ["Display the current time"] = "Exibe a hora atual",
        ["List all registered commands"] = "Lista todos os comandos registrados",
        ["Displays help for a command"] = "Exibe ajuda para um comando",
        ["Interact with your application"] = "Interage com sua aplicação",
        ["Create a new Artisan command"] = "Cria um novo comando Artisan",
        ["Create a new Artisan command (Laravel 11)"] = "Cria um novo comando Artisan (Laravel 11)",
        ["Create a new mail class"] = "Cria uma nova classe de e-mail",
        ["Create a new notification class"] = "Cria uma nova classe de notificação",
        ["Seed the database with records"] = "Popula o banco de dados com registros",
        ["Run the database migrations"] = "Executa as migrações do banco de dados",
        ["Rollback all database migrations"] = "Desfaz todas as migrações do banco de dados",
        ["Rollback the last database migration"] = "Desfaz a última migração do banco de dados",
        ["Reset and re-run all migrations"] = "Redefine e reexecuta todas as migrações",
        ["Drop all tables and re-run all migrations"] = "Remove todas as tabelas e reexecuta as migrações",
        ["Drop all tables, views, and types"] = "Remove todas as tabelas, views e tipos do banco",
        ["Create a new migration file"] = "Cria um novo arquivo de migração",
        ["Create a new model class"] = "Cria uma nova classe modelo",
        ["Create a new controller class"] = "Cria uma nova classe de controlador",
        ["Create a new resource controller"] = "Cria um novo controlador de recurso",
        ["Create a new request form class"] = "Cria uma nova classe de formulário de requisição",
        ["Create a new middleware class"] = "Cria uma nova classe de middleware",
        ["Create a new provider class"] = "Cria uma nova classe de provedor",
        ["Create a new event class"] = "Cria uma nova classe de evento",
        ["Create a new listener class"] = "Cria uma nova classe de ouvinte",
        ["Create a new job class"] = "Cria uma nova classe de job",
        ["Create a new policy class"] = "Cria uma nova classe de política",
        ["Create a new rule class"] = "Cria uma nova classe de regra",
        ["Create a new factory class"] = "Cria uma nova classe de factory",
        ["Create a new seeder class"] = "Cria uma nova classe de seeder",
        ["Create a new test class"] = "Cria uma nova classe de teste",
        ["Create a new channel class"] = "Cria uma nova classe de canal",
        ["Create a new console command"] = "Cria um novo comando de console",
        ["Create a new component class"] = "Cria uma nova classe de componente",
        ["Create a new Vue component"] = "Cria um novo componente Vue",
        ["Create a new React component"] = "Cria um novo componente React",
        ["Create a new Livewire component"] = "Cria um novo componente Livewire",
        ["Optimize the framework for better performance"] = "Otimiza o framework para melhor desempenho",
        ["Cache the framework bootstrap files"] = "Armazena em cache os arquivos de inicialização",
        ["Clear the application cache"] = "Limpa o cache da aplicação",
        ["Clear the configuration cache"] = "Limpa o cache de configuração",
        ["Clear the route cache"] = "Limpa o cache de rotas",
        ["Clear the view cache"] = "Limpa o cache de views",
        ["Clear the event cache"] = "Limpa o cache de eventos",
        ["Remove the bootstrap/cache directory"] = "Remove o diretório bootstrap/cache",
        ["Create a route cache file"] = "Cria o arquivo de cache de rotas",
        ["Create a configuration cache file"] = "Cria o arquivo de cache de configuração",
        ["List all registered routes"] = "Lista todas as rotas registradas",
        ["List scheduled tasks"] = "Lista as tarefas agendadas",
        ["Run the schedule"] = "Executa o agendador",
        ["Display the current cache status"] = "Exibe o status atual do cache",
        ["Display the last few log entries"] = "Exibe as últimas entradas de log",
        ["List all configured queues"] = "Lista todas as filas configuradas",
        ["List all failed queue jobs"] = "Lista todos os jobs de fila com falha",
        ["Flush all failed queue jobs"] = "Remove todos os jobs de fila com falha",
        ["Retry all failed queue jobs"] = "Tenta novamente todos os jobs de fila com falha",
        ["Start the queue worker"] = "Inicia o worker de fila",
        ["Restart the queue worker"] = "Reinicia o worker de fila",
        ["Create a new queue table"] = "Cria uma nova tabela de fila",
        ["Monitor the queue"] = "Monitora a fila",
        ["Create the storage symlink"] = "Cria o link simbólico de storage",
        ["Display the current maintenance status"] = "Exibe o status de manutenção",
        ["Put the application into maintenance mode"] = "Coloca a aplicação em modo de manutenção",
        ["Bring the application out of maintenance mode"] = "Remove a aplicação do modo de manutenção",
        ["Display all available notifications"] = "Exibe todas as notificações disponíveis",
        ["Send email notifications"] = "Envia notificações por e-mail",
        ["Generate a URL to a controller action"] = "Gera uma URL para uma ação de controlador",
        ["Generate the optimized link file"] = "Gera o arquivo de links otimizados",
        ["Show the application configuration"] = "Exibe a configuração da aplicação",
        ["Interact with the database"] = "Interage com o banco de dados",
        ["Create a new database migration"] = "Cria uma nova migração de banco de dados",
        ["Display the database information"] = "Exibe informações do banco de dados",
        ["Monitor the database"] = "Monitora o banco de dados",
        ["Inspect a database table"] = "Inspeciona uma tabela do banco de dados",
        ["Display the last migrations"] = "Exibe as últimas migrações",
        ["Create the migration repository"] = "Cria o repositório de migrações",
        ["Check the status of all migrations"] = "Verifica o status de todas as migrações",
        ["Create a new Eloquent model"] = "Cria um novo modelo Eloquent",
        ["Generate an optimized model"] = "Gera um modelo otimizado",
        ["Create a new observer class"] = "Cria uma nova classe de observer",
        ["Create a new resource class"] = "Cria uma nova classe de recurso",
        ["Create a new scope class"] = "Cria uma nova classe de escopo",
        ["Validate a composer.json file"] = "Valida o arquivo composer.json",
        ["Show a list of installed packages"] = "Exibe a lista de pacotes instalados",
        ["Create a scheduled task monitor"] = "Cria um monitor de tarefa agendada",
        ["Configure the application"] = "Configura a aplicação",
        ["Downlod a file from a remote server"] = "Baixa um arquivo de um servidor remoto",
        ["Upload a file to a remote server"] = "Envia um arquivo para um servidor remoto",
        ["Installing a new application in the current directory"] = "Instala uma nova aplicação no diretório atual",
        ["Make a HTTP request to a URL"] = "Faz uma requisição HTTP para uma URL",
        ["Serve the application on the PHP development server"] = "Inicia o servidor de desenvolvimento PHP",
        ["Test the application"] = "Testa a aplicação",
        ["Run the application tests"] = "Executa os testes da aplicação",
        ["Show the application changelog"] = "Exibe o changelog da aplicação",
        ["Publish something"] = "Publica algo",
        ["Show a list of available commands"] = "Exibe a lista de comandos disponíveis",
        ["List the registered event and listeners"] = "Lista os eventos e ouvintes registrados",
        ["List the registered listeners"] = "Lista os ouvintes registrados",
        ["List the registered policies"] = "Lista as políticas registradas",
        ["Interact with the config"] = "Interage com a configuração",
    };

    public ArtisanGuiViewModel()
    {
        _config = AppServices.Get<IConfigManager>();
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        InitFromContext();
        LoadHistory();
    }

    private void InitFromContext()
    {
        var path = ProjectContext.Instance.CurrentPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            _projectPath = path;
            ProjectName = ProjectContext.Instance.CurrentName ?? "";
            _ = LoadCommandsAsync();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            AllCommands.Clear();
            FilteredCommands.Clear();
            History.Clear();
            CommandsLoaded = false;
            OutputText = string.Empty;
            InitFromContext();
        });
    }

    private async Task LoadCommandsAsync()
    {
        if (string.IsNullOrWhiteSpace(_projectPath)) return;

        StatusMessage = "Carregando comandos...";
        AllCommands.Clear();
        FilteredCommands.Clear();

        try
        {
            var output = await _runner.RunAndGetOutputAsync(_projectPath, "php", "artisan list --format=json --no-ansi");
            ParseCommands(output);
            CommandsLoaded = true;
            StatusMessage = $"{AllCommands.Count} comandos carregados.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }
    }

    private void ParseCommands(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("commands", out var commands))
            {
                foreach (var cmd in commands.EnumerateArray())
                {
                    var name = cmd.GetProperty("name").GetString() ?? "";
                    var parts = name.Split(':');
                    var ns = parts.Length > 1 ? parts[0] : "";

                    var desc = cmd.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "";
                    desc = Traducoes.TryGetValue(desc, out var traducao) ? traducao : desc;

                    var command = new ArtisanCommand
                    {
                        Name = name,
                        Namespace = ns,
                        Description = desc
                    };

                    if (cmd.TryGetProperty("arguments", out var args))
                    {
                        foreach (var arg in args.EnumerateArray())
                        {
                            var argName = arg.GetProperty("name").GetString() ?? "";
                            command.Arguments.Add(argName);
                        }
                    }

                    if (cmd.TryGetProperty("options", out var opts))
                    {
                        foreach (var opt in opts.EnumerateArray())
                        {
                            var optDesc = opt.TryGetProperty("description", out var od) ? od.GetString() ?? "" : "";
                            optDesc = Traducoes.TryGetValue(optDesc, out var optTrad) ? optTrad : optDesc;

                            var option = new ArtisanOption
                            {
                                Name = opt.GetProperty("name").GetString() ?? "",
                                Description = optDesc,
                                AcceptsValue = opt.TryGetProperty("accept_value", out var av) && av.GetBoolean(),
                                IsRequired = opt.TryGetProperty("is_required", out var ir) && ir.GetBoolean(),
                                Default = opt.TryGetProperty("default", out var def) ? def.GetString() : null
                            };
                            command.Options.Add(option);
                        }
                    }

                    AllCommands.Add(command);
                }
            }

            ApplyFilter();
        }
        catch
        {
            StatusMessage = "Erro ao interpretar lista de comandos.";
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedCommandChanged(ArtisanCommand? value)
    {
        if (value != null)
        {
            CustomCommand = value.Name;
            SearchText = value.Name;
            var args = new List<string>();
            foreach (var arg in value.Arguments)
                args.Add($"--{arg}=...");
            foreach (var opt in value.Options.Where(o => o.IsRequired))
                args.Add($"--{opt.Name}=...");
            ArgumentValues = string.Join(" ", args);
        }
    }

    partial void OnCustomCommandChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            SearchText = value;
    }

    private void ApplyFilter()
    {
        FilteredCommands.Clear();
        var search = SearchText?.Trim().ToLower() ?? "";

        var items = string.IsNullOrWhiteSpace(search)
            ? AllCommands
            : AllCommands.Where(c =>
                c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));

        foreach (var cmd in items)
            FilteredCommands.Add(cmd);
    }

    [RelayCommand]
    private async Task ExecuteAsync()
    {
        var commandName = SelectedCommand?.Name ?? CustomCommand?.Trim();

        if (string.IsNullOrWhiteSpace(commandName))
        {
            StatusMessage = "Selecione um comando na lista ou digite um na caixa acima.";
            return;
        }

        if (!DangerWarningVisible && ComandosPerigosos.Contains(commandName))
        {
            DangerCommandName = commandName;
            DangerWarningVisible = true;
            return;
        }

        DangerWarningVisible = false;
        await RunCommandAsync(commandName);
    }

    [RelayCommand]
    private async Task ConfirmDangerAsync()
    {
        DangerWarningVisible = false;
        var cmd = DangerCommandName;
        DangerCommandName = string.Empty;
        await RunCommandAsync(cmd);
    }

    [RelayCommand]
    private void CancelDanger()
    {
        DangerWarningVisible = false;
        DangerCommandName = string.Empty;
        StatusMessage = "Comando cancelado.";
    }

    private void LoadHistory()
    {
        var saved = _config?.Get<string>("artisan_history");
        if (string.IsNullOrWhiteSpace(saved)) return;

        try
        {
            var items = JsonSerializer.Deserialize<List<CommandHistory>>(saved);
            if (items == null) return;
            History.Clear();
            foreach (var item in items)
                History.Add(item);
        }
        catch { }
    }

    private void SaveHistory()
    {
        try
        {
            var json = JsonSerializer.Serialize(History.Take(50).ToList());
            _config?.Set("artisan_history", json);
        }
        catch { }
    }

    private async Task RunCommandAsync(string commandName)
    {
        IsRunning = true;
        OutputText = "";
        StatusMessage = $"Executando {commandName}...";

        var args = $"artisan {commandName} {ArgumentValues}".Trim();

        try
        {
            _runner.OutputReceived += OnOutputReceived;
            _runner.ErrorReceived += OnErrorReceived;

            await _runner.RunAsync(_projectPath, "php", args);

            History.Insert(0, new CommandHistory
            {
                Command = commandName,
                Arguments = ArgumentValues,
                ExecutedAt = DateTime.Now
            });

            SaveHistory();
            StatusMessage = "Comando executado.";
        }
        catch (Exception ex)
        {
            OutputText += $"\nErro: {ex.Message}";
        }
        finally
        {
            _runner.OutputReceived -= OnOutputReceived;
            _runner.ErrorReceived -= OnErrorReceived;
            IsRunning = false;
        }
    }

    private static string StripAnsi(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        int idx;
        while ((idx = text.IndexOf('\x1b', StringComparison.Ordinal)) >= 0)
        {
            var end = text.IndexOf('m', idx);
            if (end < 0) end = text.IndexOf('K', idx);
            if (end < 0) break;
            text = text[..idx] + text[(end + 1)..];
        }
        return text;
    }

    private void OnOutputReceived(string data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            OutputText += StripAnsi(data) + "\n";
        });
    }

    private void OnErrorReceived(string data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            OutputText += $"[ERRO] {StripAnsi(data)}\n";
        });
    }

    [RelayCommand]
    private void ClearOutput()
    {
        OutputText = "";
    }

    [RelayCommand]
    private void ClearHistory()
    {
        History.Clear();
        SaveHistory();
        StatusMessage = "Histórico limpo.";
    }

    [RelayCommand]
    private async Task CopyOutputAsync()
    {
        if (string.IsNullOrWhiteSpace(OutputText)) return;
        var clipboard = TopLevel.GetTopLevel(
            Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(OutputText);
            StatusMessage = "Saída copiada para a área de transferência.";
        }
    }
}
