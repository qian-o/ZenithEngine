using Graphics.Core;
using Renderer.Models;

namespace Renderer;

internal static class App
{
    private static Window? _window;
    private static MainWindow? _mainWindow;

    public static MainWindow MainWindow => _mainWindow ?? throw new InvalidOperationException("App is not initialized.");

    public static Settings Settings { get; } = new();

    public static void Initialize()
    {
        if (_window != null)
        {
            return;
        }

        using Window window = Window.CreateWindowByVulkan();
        window.Load += Window_Load;
        window.Close += Window_Close;

        _window = window;

        window.Run();
    }

    public static void Exit()
    {
        _window?.Exit();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        _mainWindow = new MainWindow((Window)sender!);
    }

    private static void Window_Close(object? sender, CloseEventArgs e)
    {
        _mainWindow?.Dispose();
    }
}
