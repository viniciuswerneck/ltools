using Avalonia.Controls;
using Avalonia.Input;
using LTools.LogViewer.Models;
using LTools.LogViewer.ViewModels;

namespace LTools.LogViewer.Views;

public partial class LogViewerView : UserControl
{
    public LogViewerView()
    {
        InitializeComponent();
    }

    private async void OpenLog(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is LogFile logFile)
        {
            if (DataContext is LogViewerViewModel vm)
                await vm.OpenLogCommand.ExecuteAsync(logFile);
        }
    }
}