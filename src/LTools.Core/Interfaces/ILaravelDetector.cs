using LTools.Core.Models;

namespace LTools.Core.Interfaces;

public interface ILaravelDetector
{
    Task<List<LaravelProject>> ScanAsync(string rootPath);
    Task<LaravelProject?> DetectAsync(string projectPath);
    bool IsLaravelProject(string path);
}
