using CommunityToolkit.Mvvm.ComponentModel;

namespace LTools.MigrationStudio.Models;

public partial class ColumnDefinition : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _type = "string";

    [ObservableProperty]
    private int _length = 255;

    [ObservableProperty]
    private int _precision = 8;

    [ObservableProperty]
    private int _scale = 2;

    [ObservableProperty]
    private bool _nullable;

    [ObservableProperty]
    private bool _unique;

    [ObservableProperty]
    private bool _unsigned;

    [ObservableProperty]
    private bool _autoIncrement;

    [ObservableProperty]
    private bool _primary;

    [ObservableProperty]
    private string _defaultValue = string.Empty;

    [ObservableProperty]
    private bool _foreign;

    [ObservableProperty]
    private string _foreignTable = string.Empty;

    [ObservableProperty]
    private string _foreignColumn = "id";

    [ObservableProperty]
    private string _onDelete = "cascade";

    [ObservableProperty]
    private string _onUpdate = "cascade";

    [ObservableProperty]
    private string _enumValues = string.Empty;

    [ObservableProperty]
    private string _comment = string.Empty;

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "(novo campo)" : Name;

    public string DisplayInfo
    {
        get
        {
            var parts = new List<string> { Type };
            if (Nullable) parts.Add("nullable");
            if (Unique) parts.Add("unique");
            if (Primary) parts.Add("PK");
            if (Foreign) parts.Add("FK");
            return string.Join(", ", parts);
        }
    }
}
