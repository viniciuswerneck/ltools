using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.QueueMonitor.ViewModels;
using LTools.QueueMonitor.Views;

namespace LTools.QueueMonitor;

public class QueueMonitorPlugin : ILToolsPlugin
{
    public string Name => "Queue";
    public string Icon => "📨";

    private QueueMonitorView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Queue Monitor carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new QueueMonitorView
            {
                DataContext = new QueueMonitorViewModel()
            };
        }
        return _view;
    }
}