using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;

namespace LTools.EnvManager.ViewModels;

public partial class EnvManagerViewModel : ObservableObject
{
    private string _projectPath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _selectedEnvFile = string.Empty;

    [ObservableProperty]
    private string _envContent = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto no menu lateral.";

    [ObservableProperty]
    private bool _isLoaded;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    public ObservableCollection<string> EnvFiles { get; } = [];

    partial void OnEnvContentChanged(string value)
    {
        if (IsLoaded)
            HasUnsavedChanges = true;
    }

    public EnvManagerViewModel()
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
            LoadEnvFiles();
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            EnvFiles.Clear();
            IsLoaded = false;
            HasUnsavedChanges = false;
            EnvContent = string.Empty;
            InitFromContext();
        });
    }

    private void LoadEnvFiles()
    {
        EnvFiles.Clear();
        IsLoaded = false;
        HasUnsavedChanges = false;

        var patterns = new[] { ".env", ".env.example", ".env.local", ".env.dev", ".env.qa", ".env.prod", ".env.staging" };

        foreach (var pattern in patterns)
        {
            var path = Path.Combine(_projectPath, pattern);
            if (File.Exists(path))
                EnvFiles.Add(pattern);
        }

        if (EnvFiles.Count > 0)
        {
            SelectedEnvFile = EnvFiles[0];
            LoadEnvFile();
        }
        else
        {
            StatusMessage = "Nenhum arquivo .env encontrado no projeto.";
            EnvContent = string.Empty;
        }
    }

    partial void OnSelectedEnvFileChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            LoadEnvFile();
    }

    private void LoadEnvFile()
    {
        var path = Path.Combine(_projectPath, SelectedEnvFile);
        if (!File.Exists(path)) return;

        EnvContent = File.ReadAllText(path);
        IsLoaded = true;
        HasUnsavedChanges = false;
        StatusMessage = $"Editando {SelectedEnvFile}";
    }

    [RelayCommand]
    private void SaveEnv()
    {
        if (string.IsNullOrWhiteSpace(SelectedEnvFile)) return;

        var path = Path.Combine(_projectPath, SelectedEnvFile);

        try
        {
            BackupEnv(SelectedEnvFile);
            File.WriteAllText(path, EnvContent);
            HasUnsavedChanges = false;
            StatusMessage = $"{SelectedEnvFile} salvo com sucesso! Backup criado.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao salvar: {ex.Message}";
        }
    }

    private void BackupEnv(string fileName)
    {
        var sourcePath = Path.Combine(_projectPath, fileName);
        if (!File.Exists(sourcePath)) return;

        var backupDir = Path.Combine(_projectPath, ".env-backups");
        Directory.CreateDirectory(backupDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var backupName = $"{fileName}.{timestamp}.bak";
        var backupPath = Path.Combine(backupDir, backupName);

        File.Copy(sourcePath, backupPath, true);
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        var backupDir = Path.Combine(_projectPath, ".env-backups");
        if (!Directory.Exists(backupDir))
        {
            StatusMessage = "Nenhum backup encontrado.";
            return;
        }

        var backups = Directory.GetFiles(backupDir, "*.bak")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();

        if (backups.Count == 0)
        {
            StatusMessage = "Nenhum backup encontrado.";
            return;
        }

        var latest = backups.First();
        var nameWithoutBak = Path.GetFileNameWithoutExtension(latest.Name);
        var lastDot = nameWithoutBak.LastIndexOf('.');
        var originalName = lastDot > 0 ? nameWithoutBak[..lastDot] : nameWithoutBak;
        var originalPath = Path.Combine(_projectPath, originalName);

        try
        {
            var content = await File.ReadAllTextAsync(latest.FullName);
            await File.WriteAllTextAsync(originalPath, content);

            if (SelectedEnvFile == originalName)
                EnvContent = content;

            StatusMessage = $"Backup restaurado: {latest.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao restaurar: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CreateEnvFromExample()
    {
        var examplePath = Path.Combine(_projectPath, ".env.example");
        var envPath = Path.Combine(_projectPath, ".env");

        if (!File.Exists(examplePath))
        {
            StatusMessage = ".env.example não encontrado.";
            return;
        }

        if (File.Exists(envPath))
        {
            StatusMessage = ".env já existe.";
            return;
        }

        try
        {
            File.Copy(examplePath, envPath);
            LoadEnvFiles();
            SelectedEnvFile = ".env";
            StatusMessage = ".env criado a partir do .env.example";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }
    }
}
