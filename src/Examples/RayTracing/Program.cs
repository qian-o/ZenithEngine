using ZenithEngine.Common.Enums;

namespace RayTracing;

internal static class Program
{
    private static void Main(string[] _)
    {
        new RayTracingTest(Backend.DirectX12).Run();
    }
}
