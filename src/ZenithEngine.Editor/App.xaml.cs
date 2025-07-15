using Microsoft.UI.Windowing;
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
        Window window = new()
        {
            Content = new MainView()
        };

        OverlappedPresenter overlappedPresenter = OverlappedPresenter.Create();
        overlappedPresenter.SetBorderAndTitleBar(false, false);

        window.AppWindow.SetPresenter(overlappedPresenter);

        bool isfirstActivation = false;

        window.Activated += (_, _) =>
        {
            if (isfirstActivation)
            {
                return;
            }

            overlappedPresenter.Maximize();

            isfirstActivation = true;
        };

        window.Activate();
    }
}
