using ZenithEngine.Common.Enums;

namespace RayTracing;

internal class Program
{
    private static void Main(string[] _)
    {
        new RayTracingTest(Backend.Vulkan).Run();
    }
}
