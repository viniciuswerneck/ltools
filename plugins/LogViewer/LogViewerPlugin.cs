using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.LogViewer.ViewModels;
using LTools.LogViewer.Views;

namespace LTools.LogViewer;

public class LogViewerPlugin : ILToolsPlugin
{
    public string Name => "Logs";
    public string Icon => "📋";

    private LogViewerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Log Viewer carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new LogViewerView
            {
                DataContext = new LogViewerViewModel()
            };
        }
        return _view;
    }
}