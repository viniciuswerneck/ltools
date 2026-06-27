using Avalonia.Controls;
using LTools.CacheExplorer.ViewModels;
using LTools.CacheExplorer.Views;
using LTools.Core.Interfaces;
using LTools.Core.Models;

namespace LTools.CacheExplorer;

public class CacheExplorerPlugin : ILToolsPlugin
{
    public string Name => "Cache";
    public string Icon => "🗄️";

    private CacheExplorerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Cache Explorer carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new CacheExplorerView
            {
                DataContext = new CacheExplorerViewModel()
            };
        }
        return _view;
    }
}