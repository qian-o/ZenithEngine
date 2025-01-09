using ZenithEngine.Common.Enums;

namespace Triangle;

internal class Program
{
    private static unsafe void Main(string[] _)
    {
        new TriangleTest(Backend.Vulkan).Run();
    }
}
