using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Models;

namespace Renderer;

internal static class App
{
    private static Context? _context;
    private static MainWindow? _mainWindow;

    public static Context Context => _context ?? throw new InvalidOperationException("App is not initialized.");

    public static MainWindow MainWindow => _mainWindow ?? throw new InvalidOperationException("App is not initialized.");

    public static Settings Settings { get; } = new();

    public static void Initialize()
    {
        if (_context != null)
        {
            return;
        }

        using SdlWindow sdlWindow = SdlWindow.CreateWindowByVulkan();
        sdlWindow.Load += Window_Load;
        sdlWindow.Closing += Window_Closing;

        sdlWindow.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        _context = new();
        _mainWindow = new MainWindow((SdlWindow)sender!);
    }

    private static void Window_Closing(object? sender, ClosingEventArgs e)
    {
        _mainWindow?.Dispose();
        _context?.Dispose();
    }
}
