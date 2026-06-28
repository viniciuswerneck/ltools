using CommunityToolkit.Mvvm.ComponentModel;

namespace LTools.RouteExplorer.Models;

public partial class RouteInfo : ObservableObject
{
    [ObservableProperty]
    private string _method = string.Empty;

    [ObservableProperty]
    private string _uri = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _action = string.Empty;

    [ObservableProperty]
    private string _middleware = string.Empty;

    [ObservableProperty]
    private string _domain = string.Empty;

    [ObservableProperty]
    private bool _isVendor;

    public bool HasName => !string.IsNullOrWhiteSpace(Name);

    public bool HasDomain => !string.IsNullOrWhiteSpace(Domain);

    public string MethodBadgeBg => Method switch
    {
        "GET" => "#1A0078D4",
        "HEAD" => "#1A555555",
        "POST" => "#1A28A745",
        "PUT" => "#1AFFC107",
        "PATCH" => "#1AFF9800",
        "DELETE" => "#1ADC3545",
        "OPTIONS" => "#1A6F42C1",
        _ => "#1A555555"
    };

    public string MethodBadgeTextColor => Method switch
    {
        "GET" => "#4C9AFF",
        "HEAD" => "#999999",
        "POST" => "#5CBD6E",
        "PUT" => "#FFC107",
        "PATCH" => "#FF9800",
        "DELETE" => "#F06A7A",
        "OPTIONS" => "#B388FF",
        _ => "#999999"
    };

    public string MethodButtonBg => Method switch
    {
        "GET" => "#1A4C9AFF",
        "POST" => "#1A5CBD6E",
        "PUT" => "#1AFFC107",
        "PATCH" => "#1AFF9800",
        "DELETE" => "#1AF06A7A",
        _ => "#1A555555"
    };

    public string ControllerName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Action)) return "";
            var parts = Action.Split('@');
            var controller = parts[0].Trim();
            var lastDot = controller.LastIndexOf('.');
            return lastDot >= 0 ? controller[(lastDot + 1)..] : controller;
        }
    }

    public string FullController => Action.Contains('@') ? Action : "";

    public string GroupPrefix
    {
        get
        {
            var segments = Uri.Split('/');
            if (segments.Length > 1 && !string.IsNullOrWhiteSpace(segments[0]))
                return segments[0];
            return "(root)";
        }
    }

    public string? ActionMethod
    {
        get
        {
            if (!Action.Contains('@')) return null;
            return Action.Split('@')[^1];
        }
    }
    public string[] Methods => Method.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
