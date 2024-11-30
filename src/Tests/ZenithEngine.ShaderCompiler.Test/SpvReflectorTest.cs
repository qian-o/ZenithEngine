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
        Test(layout["mvp1"], ResourceType.ConstantBuffer, 1, 0, 0);
        Test(layout["mvp2"], ResourceType.StructuredBuffer, 1, 0, 0);

        // RWStructuredBuffer<float4> colors : register(u0, space1);
        // RWTexture2D<float4> texture : register(u1, space1);
        // Texture2D texture1[10] : register(t0, space1);
        // Texture2D texture2[] : register(t1, space1);
        // SamplerState samplerState[] : register(s0, space2);
        layout = SpvReflector.Reflect(DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain"));
        Test(layout["colors"], ResourceType.StructuredBufferReadWrite, 1, 0, 1);
        Test(layout["texture"], ResourceType.TextureReadWrite, 1, 1, 1);
        Test(layout["texture1"], ResourceType.Texture, 10, 0, 1);
        Test(layout["texture2"], ResourceType.Texture, 0, 1, 1);
        Test(layout["samplerState"], ResourceType.Sampler, 0, 0, 2);
    }

    private static void Test(ReflectResource resource,
                             ResourceType type,
                             uint count,
                             uint slot,
                             uint space)
    {
        Assert.AreEqual(type, resource.Type);
        Assert.AreEqual(count, resource.Count);
        Assert.AreEqual(slot, resource.Slot);
        Assert.AreEqual(space, resource.Space);
    }
}
