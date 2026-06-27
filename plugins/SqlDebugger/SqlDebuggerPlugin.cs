using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.SqlDebugger.ViewModels;
using LTools.SqlDebugger.Views;

namespace LTools.SqlDebugger;

public class SqlDebuggerPlugin : ILToolsPlugin
{
    public string Name => "SQL Debug";
    public string Icon => "🔍";

    private SqlDebuggerView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "SQL Debugger carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new SqlDebuggerView
            {
                DataContext = new SqlDebuggerViewModel()
            };
        }
        return _view;
    }
}