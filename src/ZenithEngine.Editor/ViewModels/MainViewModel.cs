using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.Dock;
using ZenithEngine.Editor.Models;

namespace ZenithEngine.Editor.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private Project? project;

    [RelayCommand]
    private void NewProject()
    {
    }

    [RelayCommand]
    private void OpenProject()
    {
    }

    [RelayCommand]
    private void SaveProject()
    {
    }

    [RelayCommand]
    private static void Exit()
    {
        App.MainWindow.Close();
    }

    [RelayCommand]
    private void FillDocument(FillDocumentEventArgs args)
    {
    }

    [RelayCommand]
    private void NewGroup(NewGroupEventArgs args)
    {
    }

    [RelayCommand]
    private void NewWindow(NewWindowEventArgs args)
    {
    }
}