using ZenithEngine.Common.Enums;

namespace Triangle;

internal static class Program
{
    private static void Main(string[] _)
    {
        Console.Write("Enter the backend (1: DirectX12, 2: Vulkan): ");

        new TriangleTest(Console.ReadLine() == "1" ? Backend.DirectX12 : Backend.Vulkan).Run();
    }
}
