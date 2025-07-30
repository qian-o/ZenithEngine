using ZenithEngine.Editor.Views;

namespace ZenithEngine.Editor;

public sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    public static Window MainWindow { get; } = new()
    {
        Title = "Zenith Engine Editor",
        Content = new MainView()
    };

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Activate();
    }
}
