using ZenithEngine.Common.Enums;

namespace Triangle;

internal class Program
{
    private static void Main(string[] _)
    {
        new TriangleTest(Backend.Vulkan).Run();
    }
}
