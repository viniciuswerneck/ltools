namespace LTools.VirtualHosts.Models;

public class VirtualHost
{
    public string ServerName { get; set; } = string.Empty;
    public string Root { get; set; } = string.Empty;
    public string ConfigPath { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool HasSsl { get; set; }
}