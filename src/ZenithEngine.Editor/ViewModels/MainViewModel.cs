using CommunityToolkit.Mvvm.ComponentModel;
using ZenithEngine.Editor.Models;

namespace ZenithEngine.Editor.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private Project? project;
}
