using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.ArtisanGui.ViewModels;
using LTools.ArtisanGui.Views;

namespace LTools.ArtisanGui;

public class ArtisanGuiPlugin : ILToolsPlugin
{
    public string Name => "Artisan";
    public string Icon => "⚡";

    private ArtisanGuiView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Artisan GUI carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new ArtisanGuiView
            {
                DataContext = new ArtisanGuiViewModel()
            };
        }
        return _view;
    }
}
