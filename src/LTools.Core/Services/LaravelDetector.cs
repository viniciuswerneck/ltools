using System.Text.Json;
using LTools.Core.Interfaces;
using LTools.Core.Models;

namespace LTools.Core.Services;

public class LaravelDetector : ILaravelDetector
{
    public async Task<List<LaravelProject>> ScanAsync(string rootPath)
    {
        var projects = new List<LaravelProject>();

        if (!Directory.Exists(rootPath))
            return projects;

        var directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

        foreach (var dir in directories)
        {
            if (IsLaravelProject(dir))
            {
                var project = await DetectAsync(dir);
                if (project != null)
                    projects.Add(project);
            }
        }

        return projects;
    }

    public async Task<LaravelProject?> DetectAsync(string projectPath)
    {
        if (!IsLaravelProject(projectPath))
            return null;

        var project = new LaravelProject
        {
            Name = Path.GetFileName(projectPath),
            Path = projectPath,
            HasArtisan = true,
            HasComposerJson = true,
            HasEnv = File.Exists(Path.Combine(projectPath, ".env")),
            LastModified = Directory.GetLastWriteTime(projectPath),
            SizeInBytes = GetProjectSize(projectPath)
        };

        await ParseComposerJsonAsync(projectPath, project);
        ParseEnvFile(projectPath, project);

        return project;
    }

    public bool IsLaravelProject(string path)
    {
        if (!Directory.Exists(path))
            return false;

        return File.Exists(Path.Combine(path, "artisan"))
            && File.Exists(Path.Combine(path, "composer.json"));
    }

    private static async Task ParseComposerJsonAsync(string projectPath, LaravelProject project)
    {
        var composerPath = Path.Combine(projectPath, "composer.json");
        if (!File.Exists(composerPath)) return;

        try
        {
            using var stream = File.OpenRead(composerPath);
            var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            if (root.TryGetProperty("require", out var require))
            {
                if (require.TryGetProperty("laravel/framework", out var lv))
                    project.LaravelVersion = lv.GetString() ?? "";

                if (require.TryGetProperty("php", out var php))
                    project.PhpVersion = php.GetString() ?? "";
            }
        }
        catch
        {
            // composer.json malformatado, ignora
        }
    }

    private static void ParseEnvFile(string projectPath, LaravelProject project)
    {
        var envPath = Path.Combine(projectPath, ".env");
        if (!File.Exists(envPath)) return;

        try
        {
            var lines = File.ReadAllLines(envPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"');

                switch (key)
                {
                    case "APP_ENV":
                        project.Environment = value;
                        break;
                    case "DB_CONNECTION":
                        project.Database = value;
                        break;
                }
            }
        }
        catch
        {
            // .env inacessivel, ignora
        }
    }

    private static long GetProjectSize(string projectPath)
    {
        try
        {
            var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "vendor", "node_modules", ".git", ".svn"
            };

            long total = 0;

            foreach (var file in Directory.EnumerateFiles(projectPath))
                total += new FileInfo(file).Length;

            foreach (var dir in Directory.EnumerateDirectories(projectPath))
            {
                var name = Path.GetFileName(dir);
                if (!excluded.Contains(name))
                    total += GetDirectorySize(dir);
            }

            return total;
        }
        catch
        {
            return 0;
        }
    }

    private static long GetDirectorySize(string path)
    {
        long total = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(path))
                total += new FileInfo(file).Length;
            foreach (var dir in Directory.EnumerateDirectories(path))
                total += GetDirectorySize(dir);
        }
        catch { }
        return total;
    }
}
