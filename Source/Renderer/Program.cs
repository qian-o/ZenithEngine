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

        foreach (PhysicalDevice item in context.GetPhysicalDevices())
        {
            Console.WriteLine(item.Name);
        }
    }
}