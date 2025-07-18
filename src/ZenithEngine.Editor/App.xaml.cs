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
        new Window()
        {
            Title = "Zenith Engine Editor",
            Content = new MainView(),
        }.Activate();
    }
}
