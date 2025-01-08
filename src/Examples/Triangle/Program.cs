using ZenithEngine.Common.Enums;

namespace Triangle;

internal class Program
{
    private static unsafe void Main(string[] _)
    {
        TriangleTest triangleTest = new(Backend.Vulkan);

        triangleTest.Run();
    }
}
