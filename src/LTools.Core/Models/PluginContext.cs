namespace LTools.Core.Models;

public class PluginContext
{
    public string ProjectPath { get; set; } = string.Empty;
    public LaravelProject? Project { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
