using ZenithEngine.Editor.Controls;
using ZenithEngine.Editor.Views;

namespace ZenithEngine.Editor;

public sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        new ZWindow(new MainView(), true).Activate();
    }
}
