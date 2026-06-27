namespace LTools.SqlDebugger.Models;

public class TableInfo
{
    public string Name { get; set; } = string.Empty;
    public string Engine { get; set; } = string.Empty;
    public string Rows { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}