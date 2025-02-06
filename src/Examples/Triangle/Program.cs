using ZenithEngine.Common.Enums;

namespace Triangle;

internal static class Program
{
    private static void Main(string[] _)
    {
        new TriangleTest(Backend.DirectX12).Run();
    }
}
