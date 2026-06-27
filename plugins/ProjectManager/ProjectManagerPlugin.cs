using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.ProjectManager.ViewModels;
using LTools.ProjectManager.Views;

namespace LTools.ProjectManager;

public class ProjectManagerPlugin : ILToolsPlugin
{
    public string Name => "Projetos";
    public string Icon => "📁";

    private ProjectManagerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Gerenciador de Projetos carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new ProjectManagerView
            {
                DataContext = new ProjectManagerViewModel()
            };
        }
        return _view;
    }
}
