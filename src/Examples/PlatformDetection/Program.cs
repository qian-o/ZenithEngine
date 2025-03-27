using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

foreach (Backend backend in Enum.GetValues<Backend>())
{
    DetectPlatform(backend);
}

static void DetectPlatform(Backend backend)
{
    Console.WriteLine($"Backend: {backend}");

    try
    {
        using GraphicsContext context = GraphicsContext.Create(backend);
        context.CreateDevice();

        Console.WriteLine($"    Device: {context.Capabilities.DeviceName}");
        Console.WriteLine($"    Ray Query: {context.Capabilities.IsRayQuerySupported}");
        Console.WriteLine($"    Ray Tracing: {context.Capabilities.IsRayTracingSupported}");
        Console.WriteLine();
    }
    catch (Exception)
    {
        Console.WriteLine("    Failed to create device.");
        Console.WriteLine();
    }
}