using ZenithEngine.Common.Enums;

namespace Triangle;

internal static class Program
{
    private static void Main(string[] _)
    {
        Console.Write("Enter the backend (0: DirectX12, 1: Vulkan): ");

        new TriangleTest(Console.ReadLine() == "0" ? Backend.DirectX12 : Backend.Vulkan).Run();
    }
}
