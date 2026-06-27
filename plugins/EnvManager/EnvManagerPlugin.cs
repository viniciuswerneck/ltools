using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.EnvManager.ViewModels;
using LTools.EnvManager.Views;

namespace LTools.EnvManager;

public class EnvManagerPlugin : ILToolsPlugin
{
    public string Name => ".env";
    public string Icon => "🔐";

    private EnvManagerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Env Manager carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new EnvManagerView
            {
                DataContext = new EnvManagerViewModel()
            };
        }
        return _view;
    }
}
