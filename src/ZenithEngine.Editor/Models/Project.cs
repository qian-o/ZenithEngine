using CommunityToolkit.Mvvm.ComponentModel;

namespace ZenithEngine.Editor.Models;

public partial class Project : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;
}
