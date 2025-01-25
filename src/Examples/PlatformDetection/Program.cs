using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace PlatformDetection;

internal static class Program
{
    private static void Main(string[] _)
    {
        foreach (Backend backend in Enum.GetValues<Backend>())
        {
            DetectPlatform(backend);
        }
    }

    private static void DetectPlatform(Backend backend)
    {
        try
        {
            using GraphicsContext context = GraphicsContext.Create(backend);
            context.CreateDevice();

            Console.WriteLine($"Backend: {backend}");
            Console.WriteLine($"    Device: {context.Capabilities.DeviceName}");
            Console.WriteLine($"    Ray Query: {context.Capabilities.IsRayQuerySupported}");
            Console.WriteLine($"    Ray Tracing: {context.Capabilities.IsRayTracingSupported}");
            Console.WriteLine();
        }
        catch (Exception)
        {
            Console.WriteLine($"Backend: {backend}");
            Console.WriteLine("    Failed to create device.");
            Console.WriteLine();
        }
    }
}
