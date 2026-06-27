using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.Dashboard.ViewModels;
using LTools.Dashboard.Views;

namespace LTools.Dashboard;

public class DashboardPlugin : ILToolsPlugin
{
    public string Name => "Dashboard";
    public string Icon => "📊";

    private DashboardView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Dashboard carregado com sucesso"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new DashboardView
            {
                DataContext = new DashboardViewModel()
            };
        }
        return _view;
    }
}
