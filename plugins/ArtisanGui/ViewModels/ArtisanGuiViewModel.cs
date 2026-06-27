using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.ArtisanGui.Models;
using LTools.Core.Services;

namespace LTools.ArtisanGui.ViewModels;

public partial class ArtisanGuiViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
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
    private string _statusMessage = "Selecione um projeto no menu lateral.";

    public ObservableCollection<ArtisanCommand> AllCommands { get; } = [];
    public ObservableCollection<ArtisanCommand> FilteredCommands { get; } = [];
    public ObservableCollection<CommandHistory> History { get; } = [];

    public ArtisanGuiViewModel()
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

                    var command = new ArtisanCommand
                    {
                        Name = name,
                        Namespace = ns,
                        Description = cmd.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : ""
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
                            var option = new ArtisanOption
                            {
                                Name = opt.GetProperty("name").GetString() ?? "",
                                Description = opt.TryGetProperty("description", out var od) ? od.GetString() ?? "" : "",
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

    private void OnOutputReceived(string data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            OutputText += data + "\n";
        });
    }

    private void OnErrorReceived(string data)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            OutputText += $"[ERRO] {data}\n";
        });
    }

    [RelayCommand]
    private void ClearOutput()
    {
        OutputText = "";
    }
}
