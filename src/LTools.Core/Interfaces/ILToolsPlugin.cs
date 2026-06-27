using Avalonia.Controls;
using LTools.Core.Models;

namespace LTools.Core.Interfaces;

public interface ILToolsPlugin
{
    string Name { get; }
    string Icon { get; }
    Task<PluginResult> ExecuteAsync(PluginContext context);
    UserControl GetView();
}
