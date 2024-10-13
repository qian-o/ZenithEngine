using Graphics.Vulkan;

namespace Tests.AndroidApp;

public partial class App : Application
{
    public static Context Context { get; private set; }

    public static GraphicsDevice Device { get; private set; }

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        Context = new Context();
        Device = Context.CreateGraphicsDevice(Context.GetBestPhysicalDevice());
    }
}
