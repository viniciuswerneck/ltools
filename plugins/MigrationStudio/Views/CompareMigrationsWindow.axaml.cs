using Avalonia.Controls;

namespace LTools.MigrationStudio.Views;

public partial class CompareMigrationsWindow : Window
{
    public CompareMigrationsWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
