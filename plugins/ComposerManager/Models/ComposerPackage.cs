namespace LTools.ComposerManager.Models;

public class ComposerPackage
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class ComposerHistory
{
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
}