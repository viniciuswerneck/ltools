using System.Collections.ObjectModel;

namespace LTools.DatabaseDiagram.Models;

public class TableSchema
{
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<ColumnSchema> Columns { get; set; } = [];
    public ObservableCollection<Relationship> Relationships { get; set; } = [];
}

public class ColumnSchema
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimary { get; set; }
    public string Default { get; set; } = string.Empty;
    public string ForeignKey { get; set; } = string.Empty;
}

public class Relationship
{
    public string FromTable { get; set; } = string.Empty;
    public string FromColumn { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string ToColumn { get; set; } = string.Empty;
}