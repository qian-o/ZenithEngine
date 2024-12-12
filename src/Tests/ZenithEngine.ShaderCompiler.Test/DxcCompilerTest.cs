using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler.Test;

[TestClass]
public class DxcCompilerTest
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

        static string IncludeHandler(string fileName)
        {
            return File.ReadAllText(Path.Combine(assetsPath, fileName));
        }
    }

    [TestMethod]
    public void TestCompileError()
    {
        string source = File.ReadAllText(Path.Combine(assetsPath, "Error.hlsl"));

        Assert.ThrowsException<InvalidOperationException>(() => DxcCompiler.Compile(ShaderStages.None, source, "Main"));
    }

    [TestMethod]
    public void TestImGuiShader()
    {
        string legacy = File.ReadAllText(Path.Combine(assetsPath, "ImGui.hlsl"));
        string linear = legacy.Replace("#if 0", "#if 1");

        byte[] vsLegacy = DxcCompiler.Compile(ShaderStages.Vertex, legacy, "VSMain");
        byte[] psLegacy = DxcCompiler.Compile(ShaderStages.Pixel, legacy, "PSMain");

        byte[] vsLinear = DxcCompiler.Compile(ShaderStages.Vertex, linear, "VSMain");
        byte[] psLinear = DxcCompiler.Compile(ShaderStages.Pixel, linear, "PSMain");

        string vsLegacyHex = Convert.ToHexString(vsLegacy);
        string psLegacyHex = Convert.ToHexString(psLegacy);

        string vsLinearHex = Convert.ToHexString(vsLinear);
        string psLinearHex = Convert.ToHexString(psLinear);

        Assert.AreNotEqual(vsLegacyHex, vsLinearHex);
        Assert.AreEqual(psLegacyHex, psLinearHex);
    }
}
