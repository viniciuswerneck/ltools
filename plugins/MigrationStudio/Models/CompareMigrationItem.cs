namespace LTools.MigrationStudio.Models;

public class CompareMigrationItem
{
    public string Name { get; init; } = string.Empty;
    public string TypeLabel { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public string StatusIcon { get; init; } = string.Empty;
    public string? Batch { get; init; }
    public string? Detail { get; init; }
}
