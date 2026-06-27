namespace LTools.ArtisanGui.Models;

public class ArtisanCommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = [];
    public List<ArtisanOption> Options { get; set; } = [];
    public bool IsFavorite { get; set; }
}

public class ArtisanOption
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool AcceptsValue { get; set; }
    public bool IsRequired { get; set; }
    public bool IsNegatable { get; set; }
    public string? Shortcut { get; set; }
    public string? Default { get; set; }
}

public class CommandHistory
{
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
}
