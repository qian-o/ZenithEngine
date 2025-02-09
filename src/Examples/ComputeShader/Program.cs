using ZenithEngine.Common.Enums;

namespace ComputeShader;

internal static class Program
{
    private static void Main(string[] _)
    {
        new ComputeShaderTest(Backend.DirectX12).Run();
    }
}
