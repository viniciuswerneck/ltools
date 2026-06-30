using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.ProjectDoctor.ViewModels;
using LTools.ProjectDoctor.Views;

namespace LTools.ProjectDoctor;

public class ProjectDoctorPlugin : ILToolsPlugin
{
    public string Name => "Diagnóstico";
    public string Icon => "🩺";

    private ProjectDoctorView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Project Doctor carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new ProjectDoctorView
            {
                DataContext = new ProjectDoctorViewModel()
            };
        }
        return _view;
    }
}