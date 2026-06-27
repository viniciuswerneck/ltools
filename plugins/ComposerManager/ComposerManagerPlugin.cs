using Avalonia.Controls;
using LTools.ComposerManager.ViewModels;
using LTools.ComposerManager.Views;
using LTools.Core.Interfaces;
using LTools.Core.Models;

namespace LTools.ComposerManager;

public class ComposerManagerPlugin : ILToolsPlugin
{
    public string Name => "Composer";
    public string Icon => "🧙";

    private ComposerManagerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Composer Manager carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new ComposerManagerView
            {
                DataContext = new ComposerManagerViewModel()
            };
        }
        return _view;
    }
}