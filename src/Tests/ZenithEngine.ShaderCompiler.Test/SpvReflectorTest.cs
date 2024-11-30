using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler.Test;

[TestClass]
public class SpvReflectorTest
{
    private static readonly string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");

    [TestMethod]
    public void TestReflect()
    {
        string source = File.ReadAllText(Path.Combine(assetsPath, "Simple.hlsl"));

        SpvReflector.Reflect(DxcCompiler.Compile(ShaderStages.Vertex, source, "VSMain"));
        SpvReflector.Reflect(DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain"));
    }
}
