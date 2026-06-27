using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.VirtualHosts.ViewModels;
using LTools.VirtualHosts.Views;

namespace LTools.VirtualHosts;

public class VirtualHostsPlugin : ILToolsPlugin
{
    public string Name => "Hosts";
    public string Icon => "🌐";

    private VirtualHostsView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Virtual Hosts carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new VirtualHostsView
            {
                DataContext = new VirtualHostsViewModel()
            };
        }
        return _view;
    }
}