using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.Scheduler.ViewModels;
using LTools.Scheduler.Views;

namespace LTools.Scheduler;

public class SchedulerPlugin : ILToolsPlugin
{
    public string Name => "Scheduler";
    public string Icon => "⏰";

    private SchedulerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Scheduler carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new SchedulerView
            {
                DataContext = new SchedulerViewModel()
            };
        }
        return _view;
    }
}