using System.Text;
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
        string source = File.ReadAllText(Path.Combine(assetsPath, "Include.hlsl"));

        Assert.IsTrue(DxcCompiler.Compile(ShaderStages.Vertex, source, "VSMain", IncludeHandler).Length > 0);
        Assert.IsTrue(DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain", IncludeHandler).Length > 0);

        static byte[] IncludeHandler(string fileName)
        {
            return Encoding.UTF8.GetBytes(File.ReadAllText(Path.Combine(assetsPath, fileName)));
        }
    }

    [TestMethod]
    public void TestCompileError()
    {
        string source = File.ReadAllText(Path.Combine(assetsPath, "Error.hlsl"));

        Assert.ThrowsException<InvalidOperationException>(() => DxcCompiler.Compile(ShaderStages.None, source, "Main"));
    }
}
