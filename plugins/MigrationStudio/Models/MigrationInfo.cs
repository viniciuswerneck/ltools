namespace LTools.MigrationStudio.Models;

public class MigrationInfo
{
    public string Name { get; init; } = string.Empty;
    public bool Ran { get; init; }
    public int? Batch { get; init; }
    public string Display => $"{Name}  [{(Ran ? $"Batch {Batch}" : "pendente")}]";
}
