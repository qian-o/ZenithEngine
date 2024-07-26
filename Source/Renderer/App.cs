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

        using GWindow gWindow = GWindow.CreateWindowByVulkan();
        gWindow.Load += Window_Load;
        gWindow.Closing += Window_Closing;

        gWindow.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        _context = new();
        _mainWindow = new MainWindow((GWindow)sender!);
    }

    private static void Window_Closing(object? sender, ClosingEventArgs e)
    {
        _mainWindow?.Dispose();
        _context?.Dispose();
    }
}
