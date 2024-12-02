using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;

namespace ZenithEngine.Test;

[TestClass]
public class GraphicsContextTest
{
    public const Backend RenderBackend = Backend.Vulkan;

    private static readonly string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");

    [TestMethod]
    public void TestCreateDevice()
    {
        AssertEx.IsConsoleErrorEmpty(() =>
        {
            using GraphicsContext context = GraphicsContext.Create(RenderBackend);

            context.CreateDevice(true);
        });
    }

    [TestMethod]
    public void TestShader()
    {
        AssertEx.IsConsoleErrorEmpty(() =>
        {
            string source = File.ReadAllText(Path.Combine(assetsPath, "Simple.hlsl"));

            byte[] vs = DxcCompiler.Compile(ShaderStages.Vertex, source, "VSMain");
            byte[] ps = DxcCompiler.Compile(ShaderStages.Pixel, source, "PSMain");

            ReflectResult result = SpvReflector.Reflect(vs);
            result = result.Merge(SpvReflector.Reflect(ps));

            using GraphicsContext context = GraphicsContext.Create(RenderBackend);

            context.CreateDevice(true);

            ShaderDesc vsDesc = ShaderDesc.Default(ShaderStages.Vertex, vs, "VSMain");
            ShaderDesc psDesc = ShaderDesc.Default(ShaderStages.Pixel, ps, "PSMain");

            using Shader vsShader = context.Factory.CreateShader(in vsDesc);
            using Shader psShader = context.Factory.CreateShader(in psDesc);

            ResourceLayoutDesc layoutDesc1 = ResourceLayoutDesc.Default(result["mvp1"].Desc,
                                                                        result["mvp2"].Desc);
            ResourceLayoutDesc layoutDesc2 = ResourceLayoutDesc.Default(result["colors"].Desc,
                                                                        result["texture"].Desc,
                                                                        result["texture1"].Desc,
                                                                        result["texture2", 20].Desc);
            ResourceLayoutDesc layoutDesc3 = ResourceLayoutDesc.Default(result["samplerState", 4].Desc);

            using ResourceLayout layout1 = context.Factory.CreateResourceLayout(in layoutDesc1);
            using ResourceLayout layout2 = context.Factory.CreateResourceLayout(in layoutDesc2);
            using ResourceLayout layout3 = context.Factory.CreateResourceLayout(in layoutDesc3);

            GraphicsShaderDesc shaderDesc = GraphicsShaderDesc.Default(vertex: vsShader, pixel: psShader);

            LayoutDesc layoutDesc = LayoutDesc.Default();
            layoutDesc.Add(ElementDesc.Default(ElementFormat.Float3, ElementSemanticType.Position));
            layoutDesc.Add(ElementDesc.Default(ElementFormat.Float4, ElementSemanticType.Color));
            layoutDesc.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.TexCoord));

            OutputDesc outputDesc = OutputDesc.Default(TextureSampleCount.Count1,
                                                       PixelFormat.D24UNormS8UInt,
                                                       PixelFormat.R8G8B8A8SNorm);

            GraphicsPipelineDesc graphicsPipelineDesc = GraphicsPipelineDesc.Default(shaderDesc,
                                                                                     [layoutDesc],
                                                                                     [layout1, layout2, layout3],
                                                                                     outputDesc);

            using GraphicsPipeline pipeline = context.Factory.CreateGraphicsPipeline(in graphicsPipelineDesc);
        });
    }
}
