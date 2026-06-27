using Avalonia.Controls;
using Avalonia.Interactivity;
using LTools.UI.ViewModels;

namespace LTools.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is MainWindowViewModel vm)
            await vm.InitializeAsync();
    }
}
