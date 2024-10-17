using CommunityToolkit.Mvvm.ComponentModel;

namespace Tests.AndroidApp.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string title = "Tests.AndroidApp";
}
