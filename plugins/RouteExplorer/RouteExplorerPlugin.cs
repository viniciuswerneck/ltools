using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.RouteExplorer.ViewModels;
using LTools.RouteExplorer.Views;

namespace LTools.RouteExplorer;

public class RouteExplorerPlugin : ILToolsPlugin
{
    public string Name => "Routes";
    public string Icon => "🗺️";

    private RouteExplorerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Route Explorer carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new RouteExplorerView
            {
                DataContext = new RouteExplorerViewModel()
            };
        }
        return _view;
    }
}