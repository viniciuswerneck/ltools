using CommunityToolkit.Mvvm.ComponentModel;

namespace LTools.SqlDebugger.Models;

public partial class SqlQuery : ObservableObject
{
    [ObservableProperty]
    private DateTime _timestamp;

    [ObservableProperty]
    private string _sql = string.Empty;

    [ObservableProperty]
    private double _duration;

    [ObservableProperty]
    private string _connection = string.Empty;

    public bool IsSlow => Duration > 1000;

    public string DurationBg => IsSlow ? "#1ADC3545" : "#1A555555";

    public string DurationColor => IsSlow ? "#F06A7A" : "#999999";

    public string TimestampDisplay => Timestamp.ToString("HH:mm:ss");

    public string DurationDisplay => $"{Duration:N0}ms";
}
