using Graphics.Vulkan;

namespace Tests.AndroidApp;

public partial class App : Application
{
    private static Context? _context;
    private static GraphicsDevice? _device;

    public static Context Context => _context ??= new();

    public static GraphicsDevice Device => _device ??= Context.CreateGraphicsDevice(Context.GetBestPhysicalDevice());

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
