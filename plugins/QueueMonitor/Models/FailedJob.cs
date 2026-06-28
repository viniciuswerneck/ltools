using CommunityToolkit.Mvvm.ComponentModel;

namespace LTools.QueueMonitor.Models;

public partial class FailedJob : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _connection = string.Empty;

    [ObservableProperty]
    private string _queue = string.Empty;

    [ObservableProperty]
    private string _jobClass = string.Empty;

    [ObservableProperty]
    private string _failedAt = string.Empty;

    [ObservableProperty]
    private string _exception = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
