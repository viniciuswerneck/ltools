using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.QueueMonitor.Models;

namespace LTools.QueueMonitor.ViewModels;

public partial class QueueMonitorViewModel : ObservableObject
{
    private readonly ProcessRunner _runner = new();
    private string _projectPath = string.Empty;
    private DispatcherTimer? _autoRefreshTimer;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private int _selectedTab;

    [ObservableProperty]
    private bool _isDashboardVisible = true;

    [ObservableProperty]
    private bool _isFailedJobsVisible;

    [ObservableProperty]
    private bool _isQueuesVisible;

    [ObservableProperty]
    private bool _isOutputVisible;

    [ObservableProperty]
    private string _failedJobsCount = "—";

    [ObservableProperty]
    private string _queueCount = "—";

    [ObservableProperty]
    private bool _hasFailedJobs;

    [ObservableProperty]
    private string _lastCheck = "—";

    public string FailedJobsSummary => $"{FailedJobsCount} falho(s)";
    public string LastCheckLabel => $"Check: {LastCheck}";
    public string FailedJobsDetailed => $"{FailedJobsCount} job(s) falho(s) pendente(s)";

    [ObservableProperty]
    private ObservableCollection<FailedJob> _failedJobs = [];

    [ObservableProperty]
    private FailedJob? _selectedFailedJob;

    [ObservableProperty]
    private string _failedJobDetail = string.Empty;

    [ObservableProperty]
    private bool _hasSelectedFailedJob;

    [ObservableProperty]
    private bool _isAutoRefreshEnabled;

    [ObservableProperty]
    private ObservableCollection<QueueInfo> _queueInfos = [];

    [ObservableProperty]
    private bool _hasQueues;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private bool _showOnboarding = true;

    [ObservableProperty]
    private bool _hasOutput;

    [ObservableProperty]
    private bool _hasProject;

    public bool HasNoProject => !HasProject;

    public QueueMonitorViewModel()
    {
        ProjectContext.Instance.ProjectChanged += OnProjectChanged;
        InitFromContext();
    }

    private void InitFromContext()
    {
        var path = ProjectContext.Instance.CurrentPath;
        HasProject = !string.IsNullOrWhiteSpace(path);
        if (HasProject)
        {
            _projectPath = path!;
            ProjectName = ProjectContext.Instance.CurrentName ?? "";
            StatusMessage = "Projeto selecionado. Use as ações abaixo para gerenciar filas.";
        }
        else
        {
            StatusMessage = "Nenhum projeto selecionado. Escolha um projeto na barra superior.";
        }
        OnPropertyChanged(nameof(HasNoProject));
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            OutputText = string.Empty;
            FailedJobs.Clear();
            QueueInfos.Clear();
            FailedJobsCount = "—";
            QueueCount = "—";
            HasFailedJobs = false;
            HasQueues = false;
            HasSelectedFailedJob = false;
            HasProject = false;
            OnPropertyChanged(nameof(HasNoProject));
            InitFromContext();
        });
    }

    private async Task RunArtisanAsync(string arguments)
    {
        if (string.IsNullOrWhiteSpace(_projectPath))
        {
            StatusMessage = "Selecione um projeto primeiro.";
            return;
        }

        IsRunning = true;
        AppendOutput($"> php artisan {arguments}\n");

        try
        {
            _runner.OutputReceived += OnOutputReceived;
            _runner.ErrorReceived += OnErrorReceived;

            await _runner.RunAsync(_projectPath, "php", $"artisan {arguments}");

            AppendOutput("\n✓ Comando concluído.\n");
        }
        catch (Exception ex)
        {
            AppendOutput($"\n✕ Erro: {ex.Message}\n");
        }
        finally
        {
            _runner.OutputReceived -= OnOutputReceived;
            _runner.ErrorReceived -= OnErrorReceived;
            IsRunning = false;
        }
    }

    private void OnOutputReceived(string data) =>
        Dispatcher.UIThread.Post(() => AppendOutput(data));

    private void OnErrorReceived(string data) =>
        Dispatcher.UIThread.Post(() => AppendOutput($"[ERRO] {data}"));

    private void AppendOutput(string text)
    {
        if (OutputText.Length > 100000)
            OutputText = OutputText[^80000..];
        OutputText += text;
        HasOutput = OutputText.Length > 0;
    }

    partial void OnOutputTextChanged(string value) =>
        HasOutput = !string.IsNullOrEmpty(value);

    partial void OnSelectedFailedJobChanged(FailedJob? value)
    {
        HasSelectedFailedJob = value != null;
        if (value != null)
        {
            FailedJobDetail = $" Job: {value.JobClass}\n"
                + $" ID: {value.Id}\n"
                + $" Connection: {value.Connection}\n"
                + $" Queue: {value.Queue}\n"
                + $" Failed at: {value.FailedAt}\n"
                + $"\n Exception \n{value.Exception}";
        }
    }

    partial void OnFailedJobsCountChanged(string value)
    {
        OnPropertyChanged(nameof(FailedJobsSummary));
        OnPropertyChanged(nameof(FailedJobsDetailed));
    }

    partial void OnLastCheckChanged(string value)
    {
        OnPropertyChanged(nameof(LastCheckLabel));
    }

    partial void OnSelectedTabChanged(int value)
    {
        IsDashboardVisible = value == 0;
        IsFailedJobsVisible = value == 1;
        IsQueuesVisible = value == 2;
        IsOutputVisible = value == 3;
    }

    partial void OnIsAutoRefreshEnabledChanged(bool value)
    {
        if (value)
        {
            _autoRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _autoRefreshTimer.Tick += async (_, _) =>
            {
                if (!IsRunning)
                    await CheckFailedAsync();
            };
            _autoRefreshTimer.Start();
            StatusMessage = "Auto-refresh ativo (30s). Falhos serão verificados automaticamente.";
        }
        else
        {
            _autoRefreshTimer?.Stop();
            _autoRefreshTimer = null;
            StatusMessage = "Auto-refresh desativado.";
        }
    }

    [RelayCommand]
    private async Task CheckFailedAsync()
    {
        ShowOnboarding = false;
        await RunArtisanAsync("queue:failed");

        var rawOutput = OutputText;

        if (rawOutput.Contains("Nenhum job falho", StringComparison.OrdinalIgnoreCase)
            || rawOutput.Contains("No failed jobs", StringComparison.OrdinalIgnoreCase))
        {
            FailedJobs.Clear();
            HasFailedJobs = false;
            FailedJobsCount = "0";
            StatusMessage = "Nenhum job falho encontrado.";
            LastCheck = DateTime.Now.ToString("HH:mm:ss");
            return;
        }

        var parsed = ParseFailedJobsTable(rawOutput);

        if (parsed.Count > 0)
        {
            FailedJobs = new ObservableCollection<FailedJob>(parsed);
            FailedJobsCount = parsed.Count.ToString();
            HasFailedJobs = true;
            StatusMessage = $"{parsed.Count} job(s) falho(s) encontrado(s). Selecione para ver detalhes.";
        }
        else
        {
            StatusMessage = "Não foi possível interpretar a saída. Veja a aba 'Output' para detalhes brutos.";
        }

        LastCheck = DateTime.Now.ToString("HH:mm:ss");
    }

    private List<FailedJob> ParseFailedJobsTable(string output)
    {
        var jobs = new List<FailedJob>();
        var lines = output.Split('\n');

        var rowBlocks = new List<List<string>>();
        List<string>? currentBlock = null;

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("+-"))
            {
                if (currentBlock?.Count > 0)
                    rowBlocks.Add(currentBlock);
                currentBlock = [];
            }
            else if (trimmed.StartsWith("|") && currentBlock != null)
            {
                currentBlock.Add(line);
            }
        }
        if (currentBlock?.Count > 0)
            rowBlocks.Add(currentBlock);

        foreach (var block in rowBlocks.Skip(1))
        {
            try
            {
                var job = ParseRow(block);
                if (job != null)
                    jobs.Add(job);
            }
            catch { }
        }

        return jobs;
    }

    private FailedJob? ParseRow(List<string> block)
    {
        if (block.Count == 0) return null;

        var first = block[0];
        var parts = first.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim()).ToArray();

        if (parts.Length < 4) return null;

        var job = new FailedJob
        {
            Id = parts[0],
            Connection = parts.ElementAtOrDefault(1) ?? "",
            Queue = parts.ElementAtOrDefault(2) ?? "",
            JobClass = parts.ElementAtOrDefault(3) ?? ""
        };

        var allText = string.Join(" ", block.Select(l => l.Trim())).Trim('|').Trim();
        var dateMatch = Regex.Match(allText,
            @"\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:\.\d+)?");

        if (dateMatch.Success)
        {
            var beforeDate = allText[..dateMatch.Index].Trim().TrimEnd('|').Trim();
            if (!string.IsNullOrWhiteSpace(beforeDate))
                job.JobClass = beforeDate;

            job.FailedAt = dateMatch.Value;
            job.Exception = allText[(dateMatch.Index + dateMatch.Length)..].Trim()
                .TrimEnd('|').Trim();
        }
        else
        {
            job.FailedAt = parts.ElementAtOrDefault(4) ?? "";
            job.Exception = parts.Length > 5
                ? string.Join(" ", parts.Skip(5)).Trim()
                : "";
        }

        return job;
    }

    [RelayCommand]
    private async Task RetryAllAsync()
    {
        if (!HasFailedJobs) return;
        AppendOutput("\n> Retentando todos os jobs falhos...\n");
        await RunArtisanAsync("queue:retry all");
        await CheckFailedAsync();
    }

    [RelayCommand]
    private async Task RetrySelectedAsync()
    {
        var selected = FailedJobs.Where(j => j.IsSelected).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "Selecione ao menos um job na lista para retentar.";
            return;
        }

        var ids = string.Join(" ", selected.Select(j => j.Id));
        AppendOutput($"\n> Retentando jobs: {ids}\n");
        await RunArtisanAsync($"queue:retry {ids}");
        await CheckFailedAsync();
    }

    [RelayCommand]
    private async Task FlushFailedAsync()
    {
        await RunArtisanAsync("queue:flush");
        FailedJobs.Clear();
        HasFailedJobs = false;
        FailedJobsCount = "0";
        StatusMessage = "Jobs falhos removidos com sucesso.";
    }

    [RelayCommand]
    private async Task CheckQueuesAsync()
    {
        await RunArtisanAsync("queue:monitor");

        var parsed = ParseQueueMonitorOutput(OutputText);
        QueueInfos = new ObservableCollection<QueueInfo>(parsed);
        HasQueues = parsed.Count > 0;
        QueueCount = parsed.Count.ToString();

        if (parsed.Count > 0)
        {
            var nonEmpty = parsed.Where(q => q.Size != "0").ToList();
            if (nonEmpty.Count > 0)
            {
                var details = string.Join(", ", nonEmpty.Select(q => $"{q.Name}: {q.Size}"));
                StatusMessage = $"{parsed.Count} fila(s). Com jobs pendentes: {details}";
            }
            else
            {
                StatusMessage = $"{parsed.Count} fila(s). Todas vazias.";
            }
        }
        else
        {
            StatusMessage = "Nenhuma fila encontrada ou queue:monitor não disponível.";
        }

        LastCheck = DateTime.Now.ToString("HH:mm:ss");
    }

    private List<QueueInfo> ParseQueueMonitorOutput(string output)
    {
        var queues = new List<QueueInfo>();
        var lines = output.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("|") || trimmed.StartsWith("| queue") || trimmed.StartsWith("+-"))
                continue;

            var parts = trimmed.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim()).ToArray();
            if (parts.Length >= 3)
            {
                queues.Add(new QueueInfo
                {
                    Name = parts[0],
                    Size = parts[1],
                    Status = parts[2]
                });
            }
        }

        return queues;
    }

    [RelayCommand]
    private async Task WorkOnceAsync() =>
        await RunArtisanAsync("queue:work --once");

    [RelayCommand]
    private async Task ListenAsync() =>
        await RunArtisanAsync("queue:listen");

    [RelayCommand]
    private async Task RestartAsync() =>
        await RunArtisanAsync("queue:restart");

    [RelayCommand]
    private async Task ShowBatchesAsync() =>
        await RunArtisanAsync("queue:batches");

    [RelayCommand]
    private void ToggleAutoRefresh() =>
        IsAutoRefreshEnabled = !IsAutoRefreshEnabled;

    [RelayCommand]
    private void DismissOnboarding() =>
        ShowOnboarding = false;

    [RelayCommand]
    private void ClearOutput() =>
        OutputText = string.Empty;

    [RelayCommand]
    private void SwitchTab(int tab) =>
        SelectedTab = tab;

    [RelayCommand]
    private async Task RefreshAllAsync()
    {
        if (!IsRunning)
        {
            await CheckFailedAsync();
            await CheckQueuesAsync();
        }
    }
}
