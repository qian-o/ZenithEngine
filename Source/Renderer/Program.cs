using Graphics.Core;
using Graphics.Vulkan;

internal sealed class Program
{
    private static GraphicsDevice _graphicsDevice = null!;

    private static void Main(string[] _)
    {
        using Window window = new();

        window.Load += Window_Load;
        window.Resize += Window_Resize;

        window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        using Context context = new();

        foreach (PhysicalDevice physicalDevice in context.EnumeratePhysicalDevices())
        {
            Console.WriteLine(physicalDevice.Name);

            _graphicsDevice = context.CreateGraphicsDevice(physicalDevice, (Window)sender!);

            break;
        }
    }

    private static void Window_Resize(object? sender, ResizeEventArgs e)
    {
        _graphicsDevice.Resize(e.Width, e.Height);
    }
}