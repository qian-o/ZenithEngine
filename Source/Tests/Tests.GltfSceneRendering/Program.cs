using Graphics.Core;
using Graphics.Vulkan;

internal sealed unsafe class Program
{
    private static GraphicsDevice _device = null!;

    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Title = "Tests.MultiViewports";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.EnumeratePhysicalDevices().First(), window);

        _device = device;

        window.Load += Window_Load;
        window.Update += Window_Update;
        window.Render += Window_Render;
        window.Resize += (_, e) => _device.MainSwapchain.Resize(e.Width, e.Height);
        window.Closing += Window_Closing;

        window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {

    }

    private static void Window_Update(object? sender, UpdateEventArgs e)
    {
    }

    private static void Window_Render(object? sender, RenderEventArgs e)
    {
    }

    private static void Window_Closing(object? sender, ClosingEventArgs e)
    {
    }
}