using CommunityToolkit.Mvvm.ComponentModel;

namespace LTools.QueueMonitor.Models;

public partial class QueueInfo : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _size = "0";

    [ObservableProperty]
    private string _status = "unknown";
}
