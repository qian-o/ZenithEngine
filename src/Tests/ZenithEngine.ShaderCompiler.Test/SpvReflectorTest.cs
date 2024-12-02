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

        // ConstantBuffer<MVP> mvp1 : register(b0);
        // StructuredBuffer<float4x4> mvp2 : register(t0);
        // RWStructuredBuffer<float4> colors : register(u0, space1);
        // RWTexture2D<float4> texture : register(u1, space1);
        // Texture2D texture1[10] : register(t0, space1);
        // Texture2D texture2[] : register(t1, space1);
        // SamplerState samplerState[] : register(s0, space2);
        AssertEx.AreEqual(ShaderStages.Vertex,
                          ResourceType.ConstantBuffer, 0, 0,
                          "mvp1", 1,
                          layout["mvp1"]);

        AssertEx.AreEqual(ShaderStages.Vertex | ShaderStages.Pixel,
                          ResourceType.StructuredBuffer, 0, 0,
                          "mvp2", 1,
                          layout["mvp2"]);

        AssertEx.AreEqual(ShaderStages.Pixel,
                          ResourceType.StructuredBufferReadWrite, 0, 1,
                          "colors", 1,
                          layout["colors"]);

        AssertEx.AreEqual(ShaderStages.Pixel,
                          ResourceType.TextureReadWrite, 1, 1,
                          "texture", 1,
                          layout["texture"]);

        AssertEx.AreEqual(ShaderStages.Pixel,
                          ResourceType.Texture, 0, 1,
                          "texture1", 10,
                          layout["texture1"]);

        AssertEx.AreEqual(ShaderStages.Pixel,
                          ResourceType.Texture, 1, 1,
                          "texture2", 100,
                          layout["texture2", 100]);

        AssertEx.AreEqual(ShaderStages.Pixel,
                          ResourceType.Sampler, 0, 2,
                          "samplerState", 4,
                          layout["samplerState", 4]);
    }
}
