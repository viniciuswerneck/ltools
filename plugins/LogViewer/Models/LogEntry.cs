using CommunityToolkit.Mvvm.ComponentModel;

namespace LTools.LogViewer.Models;

public partial class LogEntry : ObservableObject
{
    [ObservableProperty]
    private int _lineNumber;

    [ObservableProperty]
    private DateTime? _timestamp;

    [ObservableProperty]
    private string _level = string.Empty;

    [ObservableProperty]
    private string _environment = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _stackTrace = string.Empty;

    [ObservableProperty]
    private string _fullContent = string.Empty;

    [ObservableProperty]
    private bool _isExpanded;

    public string ShortMessage => Message.Length > 300 ? Message[..300] + "..." : Message;

    public string LevelBadge => Level switch
    {
        "EMERGENCY" => "#9C27B0",
        "ALERT" => "#C62828",
        "CRITICAL" => "#D32F2F",
        "ERROR" => "#F44336",
        "WARNING" => "#FF9800",
        "NOTICE" => "#FFC107",
        "INFO" => "#2196F3",
        "DEBUG" => "#78909C",
        _ => "#757575"
    };

    public string LevelBadgeText => Level switch
    {
        "EMERGENCY" => "EMERG",
        "ALERT" => "ALERT",
        "CRITICAL" => "CRIT",
        "ERROR" => "ERROR",
        "WARNING" => "WARN",
        "NOTICE" => "NOTE",
        "INFO" => "INFO",
        "DEBUG" => "DEBUG",
        _ => Level
    };

    public bool HasStackTrace => !string.IsNullOrWhiteSpace(StackTrace);

    public string FormattedTimestamp => Timestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "---";

    public string LevelBadgeTextColor
    {
        get
        {
            var darkLevels = new[] { "NOTICE", "WARNING" };
            return darkLevels.Contains(Level) ? "#1a1a1a" : "#ffffff";
        }
    }
}
