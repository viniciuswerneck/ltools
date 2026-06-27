namespace LTools.Core.Models;

public class LaravelProject
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string LaravelVersion { get; set; } = string.Empty;
    public string PhpVersion { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public bool HasArtisan { get; set; }
    public bool HasComposerJson { get; set; }
    public bool HasEnv { get; set; }
    public DateTime LastModified { get; set; }
}
