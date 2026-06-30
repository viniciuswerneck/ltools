using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.DatabaseDiagram.ViewModels;
using LTools.DatabaseDiagram.Views;

namespace LTools.DatabaseDiagram;

public class DatabaseDiagramPlugin : ILToolsPlugin
{
    public string Name => "Diagrama BD";
    public string Icon => "📊";

    private DatabaseDiagramView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Database Diagram carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new DatabaseDiagramView
            {
                DataContext = new DatabaseDiagramViewModel()
            };
        }
        return _view;
    }
}