using Graphics.Core;

namespace Renderer;

internal static class App
{
    private static MainWindow? _mainWindow;

    public static MainWindow MainWindow => _mainWindow ?? throw new InvalidOperationException("App is not initialized.");

    public static void Initialize()
    {
        if (_mainWindow != null)
        {
            return;
        }

        using Window window = Window.CreateWindowByVulkan();
        window.Load += Window_Load;
        window.Close += Window_Close;

        window.Run();
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
