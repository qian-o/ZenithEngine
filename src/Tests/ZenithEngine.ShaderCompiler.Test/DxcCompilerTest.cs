using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler.Test;

[TestClass]
public sealed class DxcCompilerTest
{
    private static readonly string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");

    [TestMethod]
    public void TestCompile()
    {
        string source = File.ReadAllText(Path.Combine(assetsPath, "Simple.hlsl"));

        Assert.IsTrue(DxcCompiler.Compile(ShaderStages.Vertex, source, "VSMain").Length > 0);

        Assert.IsTrue(DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain").Length > 0);
    }

    [TestMethod]
    public void TestCompileWithIncludeHandler()
    {
    }

    [TestMethod]
    public void TestCompileError()
    {
        string source = File.ReadAllText(Path.Combine(assetsPath, "Error.hlsl"));

        Assert.ThrowsException<InvalidOperationException>(() => DxcCompiler.Compile(ShaderStages.None, source, "Main"));
    }
}
