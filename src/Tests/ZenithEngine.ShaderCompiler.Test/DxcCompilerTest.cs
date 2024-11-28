namespace ZenithEngine.ShaderCompiler.Test;

[TestClass]
public sealed class DxcCompilerTest
{
    private static readonly string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");

    [TestMethod]
    public void TestCompile()
    {
        string source = File.ReadAllText(Path.Combine(assetsPath, "Simple.hlsl"));

        ReadOnlySpan<byte> spv = DxcCompiler.Compile(Common.Enums.ShaderStages.Vertex, source, "VSMain");

        Assert.IsTrue(spv.Length > 0);
    }
}
