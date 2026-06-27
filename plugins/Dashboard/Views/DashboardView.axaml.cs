using Avalonia.Controls;
using Avalonia.Interactivity;
using LTools.Dashboard.ViewModels;

namespace LTools.Dashboard.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is DashboardViewModel vm)
            await vm.LoadAsync();
    }
}
