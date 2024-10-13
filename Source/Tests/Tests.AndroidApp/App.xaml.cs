using Graphics.Vulkan;

namespace Tests.AndroidApp;

public partial class App : Application
{
    private static Context? _context;
    private static GraphicsDevice? _device;

    public static Context Context => _context ?? throw new InvalidOperationException("Context is not initialized.");

    public static GraphicsDevice Device => _device ?? throw new InvalidOperationException("Graphics device is not initialized.");

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        _context = new Context();
        _device = _context.CreateGraphicsDevice(_context.GetBestPhysicalDevice());
    }
}
