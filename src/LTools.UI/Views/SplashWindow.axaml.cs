using Avalonia.Controls;
using Avalonia.Threading;
using LTools.UI.ViewModels;

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

        var main = new MainWindow
        {
            DataContext = new MainWindowViewModel(),
        };
        main.Show();

        Close();
    }
}
