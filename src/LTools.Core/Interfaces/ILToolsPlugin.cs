using Avalonia.Controls;
using LTools.Core.Models;

namespace LTools.Core.Interfaces;

/// <summary>Defines a plugin that can be loaded by the PluginLoader and displayed in the UI.</summary>
public interface ILToolsPlugin
{
    /// <summary>Display name shown in the navigation pane.</summary>
    string Name { get; }
    /// <summary>Emoji icon for the navigation pane item.</summary>
    string Icon { get; }
    /// <summary>Executes the plugin's main action with the given context.</summary>
    Task<PluginResult> ExecuteAsync(PluginContext context);
    /// <summary>Returns the Avalonia UserControl to render in the content area.</summary>
    UserControl GetView();
}
