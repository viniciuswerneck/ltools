using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LTools.Core.Services;
using LTools.VirtualHosts.Models;

namespace LTools.VirtualHosts.ViewModels;

public partial class VirtualHostsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "Selecione um projeto na barra superior.";

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _serverName = string.Empty;

    [ObservableProperty]
    private string _rootPath = string.Empty;

    [ObservableProperty]
    private bool _enableSsl;

    private string _projectPath = string.Empty;

    public ObservableCollection<VirtualHost> Hosts { get; } = [];

    public VirtualHostsViewModel()
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
            RootPath = _projectPath;
            ServerName = $"{ProjectName}.test";
            StatusMessage = $"Projeto selecionado: {ProjectName}";
        }
    }

    private void OnProjectChanged()
    {
        Dispatcher.UIThread.Post(() => InitFromContext());
    }

    [RelayCommand]
    private async Task CreateHostAsync()
    {
        if (string.IsNullOrWhiteSpace(ServerName) || string.IsNullOrWhiteSpace(RootPath))
        {
            StatusMessage = "Preencha o nome do servidor e o diretório raiz.";
            return;
        }

        try
        {
            var config = $@"
<VirtualHost *:80>
    ServerName {ServerName}
    DocumentRoot {RootPath}/public
    <Directory {RootPath}/public>
        Options Indexes FollowSymLinks
        AllowOverride All
        Require all granted
    </Directory>
    ErrorLog ${{APACHE_LOG_DIR}}/{ServerName}_error.log
    CustomLog ${{APACHE_LOG_DIR}}/{ServerName}_access.log combined
</VirtualHost>";

            if (EnableSsl)
            {
                config += $@"
<VirtualHost *:443>
    ServerName {ServerName}
    DocumentRoot {RootPath}/public
    <Directory {RootPath}/public>
        Options Indexes FollowSymLinks
        AllowOverride All
        Require all granted
    </Directory>
    SSLEngine on
    SSLCertificateFile ${{APACHE_LOG_DIR}}/{ServerName}.crt
    SSLCertificateKeyFile ${{APACHE_LOG_DIR}}/{ServerName}.key
    ErrorLog ${{APACHE_LOG_DIR}}/{ServerName}_ssl_error.log
    CustomLog ${{APACHE_LOG_DIR}}/{ServerName}_ssl_access.log combined
</VirtualHost>";
            }

            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "vhosts", $"{ServerName}.conf");
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            await File.WriteAllTextAsync(configPath, config.Trim());

            Hosts.Add(new VirtualHost
            {
                ServerName = ServerName,
                Root = RootPath,
                ConfigPath = configPath,
                IsEnabled = true,
                HasSsl = EnableSsl
            });

            StatusMessage = $"Host {ServerName} criado em {configPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }
    }

    [RelayCommand]
    private Task DeleteHostAsync(VirtualHost? host)
    {
        if (host == null) return Task.CompletedTask;

        try
        {
            if (host.ConfigPath != null && File.Exists(host.ConfigPath))
                File.Delete(host.ConfigPath);

            Hosts.Remove(host);
            StatusMessage = $"Host {host.ServerName} removido.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
        }

        return Task.CompletedTask;
    }
}