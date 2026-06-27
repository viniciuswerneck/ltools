namespace LTools.LogViewer.Models;

public class LogFile
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string SizeFormatted => Size switch
    {
        < 1024 => $"{Size} B",
        < 1048576 => $"{Size / 1024.0:F1} KB",
        _ => $"{Size / 1048576.0:F1} MB"
    };
}