using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.DockerManager.ViewModels;
using LTools.DockerManager.Views;

namespace LTools.DockerManager;

public class DockerManagerPlugin : ILToolsPlugin
{
    public string Name => "Docker";
    public string Icon => "🐳";

    private DockerManagerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Docker Manager carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new DockerManagerView
            {
                DataContext = new DockerManagerViewModel()
            };
        }
        return _view;
    }
}