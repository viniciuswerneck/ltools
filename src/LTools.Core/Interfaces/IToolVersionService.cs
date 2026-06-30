namespace LTools.Core.Interfaces;

/// <summary>Queries installed tool versions by running shell commands.</summary>
public interface IToolVersionService
{
    /// <summary>Runs an arbitrary command with --version and returns the output.</summary>
    Task<string> GetVersionAsync(string command, string arguments = "--version");
    /// <summary>Returns the installed PHP version string.</summary>
    Task<string> GetPhpVersionAsync();
    /// <summary>Returns the installed Composer version string.</summary>
    Task<string> GetComposerVersionAsync();
    /// <summary>Returns the installed Git version string.</summary>
    Task<string> GetGitVersionAsync();
    /// <summary>Returns the installed Node.js version string.</summary>
    Task<string> GetNodeVersionAsync();
    /// <summary>Returns the installed npm version string.</summary>
    Task<string> GetNpmVersionAsync();
    /// <summary>Returns the installed Docker version string.</summary>
    Task<string> GetDockerVersionAsync();
    /// <summary>Returns the installed Laravel CLI version string.</summary>
    Task<string> GetLaravelCliVersionAsync();
}