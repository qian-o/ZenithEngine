﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tests.AndroidApp.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [RelayCommand]
    private static void ShowShell()
    {
    }
}
