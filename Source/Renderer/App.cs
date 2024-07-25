using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Models;

namespace Renderer;

internal static class App
{
    private static GraphicsWindow? _graphicsWindow;
    private static Context? _context;
    private static MainWindow? _mainWindow;

    public static Context Context => _context ?? throw new InvalidOperationException("App is not initialized.");

    public static MainWindow MainWindow => _mainWindow ?? throw new InvalidOperationException("App is not initialized.");

    public static Settings Settings { get; } = new();

    public static void Initialize()
    {
        if (_graphicsWindow != null)
        {
            return;
        }

        using GraphicsWindow graphicsWindow = GraphicsWindow.CreateWindowByVulkan();
        graphicsWindow.Load += Window_Load;
        graphicsWindow.Closing += Window_Closing;

        _graphicsWindow = graphicsWindow;

        graphicsWindow.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        _context = new();
        _mainWindow = new MainWindow((GraphicsWindow)sender!);
    }

    private static void Window_Closing(object? sender, ClosingEventArgs e)
    {
        _mainWindow?.Dispose();
        _context?.Dispose();
    }
}
