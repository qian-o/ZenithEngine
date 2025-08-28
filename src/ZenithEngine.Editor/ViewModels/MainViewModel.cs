using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.Dock;
using ZenithEngine.Editor.Models;

namespace ZenithEngine.Editor.ViewModels;

public partial class MainViewModel : ObservableRecipient, IDockAdapter, IDockBehavior
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

    void IDockAdapter.OnCreated(Document document)
    {
    }

    void IDockAdapter.OnCreated(DocumentGroup group, Document? draggedDocument)
    {
    }

    object? IDockAdapter.GetFloatingWindowTitleBar(Document? draggedDocument)
    {
        return null;
    }

    void IDockBehavior.ActivateMainWindow()
    {
        App.MainWindow.Activate();
    }

    void IDockBehavior.OnDocked(Document src, DockManager dest, DockTarget target)
    {
        Debug.WriteLine($"Document '{src.ActualTitle}' docked to DockManager at target '{target}'.");
    }

    void IDockBehavior.OnDocked(Document src, DocumentGroup dest, DockTarget target)
    {
        Debug.WriteLine($"Document '{src.ActualTitle}' docked to DocumentGroup at target '{target}'.");
    }

    void IDockBehavior.OnFloating(Document document)
    {
        Debug.WriteLine($"Document '{document.ActualTitle}' is now floating.");
    }
}