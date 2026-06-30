using Avalonia.Controls;
using LTools.Core.Interfaces;
using LTools.Core.Models;
using LTools.MigrationStudio.ViewModels;
using LTools.MigrationStudio.Views;

namespace LTools.MigrationStudio;

public class MigrationStudioPlugin : ILToolsPlugin
{
    public string Name => "Migrações";
    public string Icon => "🗄️";

    private MigrationStudioView? _view;

    public Task<PluginResult> ExecuteAsync(PluginContext context)
    {
        return Task.FromResult(new PluginResult
        {
            Success = true,
            Message = "Migration Studio carregado"
        });
    }

    public UserControl GetView()
    {
        if (_view == null)
        {
            _view = new MigrationStudioView
            {
                DataContext = new MigrationStudioViewModel()
            };
        }
        return _view;
    }
}
