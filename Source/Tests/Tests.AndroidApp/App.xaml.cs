using Graphics.Vulkan;

namespace Tests.AndroidApp;

public partial class App : Application
{
    public static Context Context { get; }

    public static GraphicsDevice Device { get; }

    static App()
    {
        Context = new Context();
        Device = Context.CreateGraphicsDevice(Context.GetBestPhysicalDevice());
    }

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
