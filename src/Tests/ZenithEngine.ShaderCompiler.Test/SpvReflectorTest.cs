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

        byte[] vs = DxcCompiler.Compile(ShaderStages.Vertex, source, "VSMain");
        byte[] ps = DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain");

        ReflectResourceLayout layout = SpvReflector.Reflect(vs);
        layout = layout.Merge(SpvReflector.Reflect(ps));
    }
}
