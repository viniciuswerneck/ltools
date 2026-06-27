namespace LTools.Scheduler.Models;

public class ScheduledTask
{
    public string Command { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string NextDue { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
}