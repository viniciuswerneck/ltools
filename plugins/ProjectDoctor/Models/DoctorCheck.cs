namespace LTools.ProjectDoctor.Models;

public enum CheckStatus
{
    Pending,
    Passed,
    Failed,
    Warning
}

public enum CheckSeverity
{
    Critical,
    Warning,
    Info
}

public class DoctorCheck : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string WhatItChecks { get; set; } = string.Empty;
    public CheckSeverity Severity { get; set; } = CheckSeverity.Warning;

    private CheckStatus _status = CheckStatus.Pending;
    public CheckStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(Icon));
                OnPropertyChanged(nameof(ShowFixButton));
            }
        }
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string? Suggestion { get; set; }
    public string? FixCommand { get; set; }

    public string Icon => Status switch
    {
        CheckStatus.Passed => "✅",
        CheckStatus.Failed => "❌",
        CheckStatus.Warning => "⚠️",
        _ => "⏳"
    };

    public string SeverityIcon => Severity switch
    {
        CheckSeverity.Critical => "🔴",
        CheckSeverity.Warning => "🟡",
        _ => "🔵"
    };

    public bool CanFix => !string.IsNullOrWhiteSpace(FixCommand);
    public bool ShowFixButton => CanFix && Status != CheckStatus.Passed && Status != CheckStatus.Pending;
    public bool IsSafeFix => Severity != CheckSeverity.Critical;

    public string FixButtonText => IsFixing ? "Corrigindo..." : "Corrigir";

    private bool _isFixing;
    public bool IsFixing
    {
        get => _isFixing;
        set
        {
            if (SetProperty(ref _isFixing, value))
                OnPropertyChanged(nameof(FixButtonText));
        }
    }

    public double Weight => Severity switch
    {
        CheckSeverity.Critical => 3.0,
        CheckSeverity.Warning => 1.5,
        _ => 1.0
    };
}
