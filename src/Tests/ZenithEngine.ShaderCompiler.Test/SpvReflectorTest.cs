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

        // ConstantBuffer<MVP> mvp1 : register(b0);
        // StructuredBuffer<float4x4> mvp2 : register(t0);
        ReflectResourceLayout layout = SpvReflector.Reflect(DxcCompiler.Compile(ShaderStages.Vertex, source, "VSMain"));

        Assert.AreEqual<uint>(0, layout["mvp1"].Space);
        Assert.AreEqual<uint>(0, layout["mvp1"].Slot);
        Assert.AreEqual(ResourceType.ConstantBuffer, layout["mvp1"].Type);
        Assert.AreEqual<uint>(1, layout["mvp1"].Count);

        Assert.AreEqual<uint>(0, layout["mvp2"].Space);
        Assert.AreEqual<uint>(0, layout["mvp2"].Slot);
        Assert.AreEqual(ResourceType.StructuredBuffer, layout["mvp2"].Type);
        Assert.AreEqual<uint>(1, layout["mvp2"].Count);

        // RWStructuredBuffer<float4> colors : register(u0, space1);
        // RWTexture2D<float4> texture : register(u1, space1);
        // Texture2D texture1[10] : register(t0, space1);
        // Texture2D texture2[] : register(t1, space1);
        // SamplerState samplerState[] : register(s0, space2);
        layout = SpvReflector.Reflect(DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain"));

        Assert.AreEqual<uint>(1, layout["colors"].Space);
        Assert.AreEqual<uint>(0, layout["colors"].Slot);
        Assert.AreEqual(ResourceType.StructuredBufferReadWrite, layout["colors"].Type);
        Assert.AreEqual<uint>(1, layout["colors"].Count);

        Assert.AreEqual<uint>(1, layout["texture"].Space);
        Assert.AreEqual<uint>(1, layout["texture"].Slot);
        Assert.AreEqual(ResourceType.TextureReadWrite, layout["texture"].Type);
        Assert.AreEqual<uint>(1, layout["texture"].Count);

        Assert.AreEqual<uint>(1, layout["texture1"].Space);
        Assert.AreEqual<uint>(0, layout["texture1"].Slot);
        Assert.AreEqual(ResourceType.Texture, layout["texture1"].Type);
        Assert.AreEqual<uint>(10, layout["texture1"].Count);

        Assert.AreEqual<uint>(1, layout["texture2"].Space);
        Assert.AreEqual<uint>(1, layout["texture2"].Slot);
        Assert.AreEqual(ResourceType.Texture, layout["texture2"].Type);
        Assert.AreEqual<uint>(0, layout["texture2"].Count);

        Assert.AreEqual<uint>(2, layout["samplerState"].Space);
        Assert.AreEqual<uint>(0, layout["samplerState"].Slot);
        Assert.AreEqual(ResourceType.Sampler, layout["samplerState"].Type);
        Assert.AreEqual<uint>(0, layout["samplerState"].Count);
    }
}
