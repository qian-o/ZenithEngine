using Graphics.Core;
using Graphics.Vulkan;

internal sealed class Program
{
    private static void Main(string[] _)
    {
        using Window window = new();

        window.Load += Window_Load;

        window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        using Context context = new();

        foreach (PhysicalDevice physicalDevice in context.EnumeratePhysicalDevices())
        {
            Console.WriteLine(physicalDevice.Name);

            GraphicsDevice graphicsDevice = context.CreateGraphicsDevice(physicalDevice, (Window)sender!);

            Console.WriteLine(graphicsDevice);
        }
    }
}