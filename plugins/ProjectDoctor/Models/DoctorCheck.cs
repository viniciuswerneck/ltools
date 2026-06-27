namespace LTools.ProjectDoctor.Models;

public class DoctorCheck
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
}