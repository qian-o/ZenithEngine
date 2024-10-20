using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tests.AndroidApp.ViewModels;

public partial class SampleViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isSettingsVisible;

    [RelayCommand]
    private void ShowSettings()
    {
        IsSettingsVisible = true;
    }
}
