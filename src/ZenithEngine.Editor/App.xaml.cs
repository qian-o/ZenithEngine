using Windows.ApplicationModel.Activation;

namespace ZenithEngine.Editor;

public sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnActivated(IActivatedEventArgs args)
    {
        Window window = new();

        window.Activate();
    }
}
