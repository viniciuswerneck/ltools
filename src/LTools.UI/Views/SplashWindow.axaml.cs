using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LTools.Core.Services;
using LTools.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LTools.UI.Views;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
    }

    public async void ShowAndThenOpenMain()
    {
        Show();

        await Task.Delay(1500);

        var vm = AppServices.GetRequired<MainWindowViewModel>();
        var main = new MainWindow
        {
            DataContext = vm,
        };
        main.Show();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = main;

        Close();
    }
}