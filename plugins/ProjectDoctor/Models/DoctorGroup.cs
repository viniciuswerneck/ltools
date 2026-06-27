using System.Collections.ObjectModel;

namespace LTools.ProjectDoctor.Models;

public class DoctorGroup
{
    public string Category { get; set; } = string.Empty;
    public ObservableCollection<DoctorCheck> Items { get; set; } = [];
}
