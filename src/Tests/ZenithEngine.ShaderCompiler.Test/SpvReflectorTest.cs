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

        ReflectResourceLayout layout = SpvReflector.Reflect(DxcCompiler.Compile(ShaderStages.Vertex, source, "VSMain"));

        layout = SpvReflector.Reflect(DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain"));
    }
}
