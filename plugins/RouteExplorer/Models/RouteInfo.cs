namespace LTools.RouteExplorer.Models;

public class RouteInfo
{
    public string Method { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Middleware { get; set; } = string.Empty;
}