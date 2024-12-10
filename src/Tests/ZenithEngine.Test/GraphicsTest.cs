using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace ZenithEngine.Test;

[TestClass]
public class GraphicsTest
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
    public void TestFactory()
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

            BufferDesc mvp1Desc = BufferDesc.Default(64, BufferUsage.ConstantBuffer);
            BufferDesc mvp2Desc = BufferDesc.Default(64, BufferUsage.StorageBuffer);
            BufferDesc colorsDesc = BufferDesc.Default(16, BufferUsage.StorageBufferReadWrite);

            using Buffer mvp1 = context.Factory.CreateBuffer(in mvp1Desc);
            using Buffer mvp2 = context.Factory.CreateBuffer(in mvp2Desc);
            using Buffer colors = context.Factory.CreateBuffer(in colorsDesc);

            TextureDesc textureDesc = TextureDesc.Default(512, 512, 1, 1, usage: TextureUsage.Storage);
            TextureDesc[] texture1Desc = new TextureDesc[10];
            Array.Fill(texture1Desc, TextureDesc.Default(512, 512, 1, 1, usage: TextureUsage.Sampled));
            TextureDesc[] texture2Desc = new TextureDesc[100];
            Array.Fill(texture2Desc, TextureDesc.Default(512, 512, 1, 1, usage: TextureUsage.Sampled));

            using Texture texture = context.Factory.CreateTexture(in textureDesc);
            Texture[] texture1 = Array.ConvertAll(texture1Desc, desc => context.Factory.CreateTexture(in desc));
            Texture[] texture2 = Array.ConvertAll(texture2Desc, desc => context.Factory.CreateTexture(in desc));

            TextureViewDesc textureViewDesc = TextureViewDesc.Default(texture);
            TextureViewDesc[] texture1ViewDesc = new TextureViewDesc[10];
            for (int i = 0; i < 10; i++)
            {
                texture1ViewDesc[i] = TextureViewDesc.Default(texture1[i]);
            }
            TextureViewDesc[] texture2ViewDesc = new TextureViewDesc[100];
            for (int i = 0; i < 100; i++)
            {
                texture2ViewDesc[i] = TextureViewDesc.Default(texture2[i]);
            }

            using TextureView textureView = context.Factory.CreateTextureView(in textureViewDesc);
            TextureView[] texture1View = Array.ConvertAll(texture1ViewDesc, desc => context.Factory.CreateTextureView(in desc));
            TextureView[] texture2View = Array.ConvertAll(texture2ViewDesc, desc => context.Factory.CreateTextureView(in desc));

            SamplerDesc[] samplerStateDesc = new SamplerDesc[4];
            Array.Fill(samplerStateDesc, SamplerDesc.Default());

            Sampler[] samplerState = Array.ConvertAll(samplerStateDesc, desc => context.Factory.CreateSampler(in desc));

            ResourceLayoutDesc layoutDesc1 = ResourceLayoutDesc.Default(result["mvp1"].Desc,
                                                                        result["mvp2"].Desc);
            ResourceLayoutDesc layoutDesc2 = ResourceLayoutDesc.Default(result["colors"].Desc,
                                                                        result["texture"].Desc,
                                                                        result["texture1"].Desc,
                                                                        result["texture2", 100].Desc);
            ResourceLayoutDesc layoutDesc3 = ResourceLayoutDesc.Default(result["samplerState", 4].Desc);

            using ResourceLayout layout1 = context.Factory.CreateResourceLayout(in layoutDesc1);
            using ResourceLayout layout2 = context.Factory.CreateResourceLayout(in layoutDesc2);
            using ResourceLayout layout3 = context.Factory.CreateResourceLayout(in layoutDesc3);

            ResourceSetDesc resourceSetDesc1 = ResourceSetDesc.Default(layout1, mvp1, mvp2);
            ResourceSetDesc resourceSetDesc2 = ResourceSetDesc.Default(layout2, [colors, textureView, .. texture1View, .. texture2View]);
            ResourceSetDesc resourceSetDesc3 = ResourceSetDesc.Default(layout3, samplerState);

            using ResourceSet resourceSet1 = context.Factory.CreateResourceSet(in resourceSetDesc1);
            using ResourceSet resourceSet2 = context.Factory.CreateResourceSet(in resourceSetDesc2);
            using ResourceSet resourceSet3 = context.Factory.CreateResourceSet(in resourceSetDesc3);

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

            foreach (TextureView item in texture1View)
            {
                item.Dispose();
            }

            foreach (TextureView item in texture2View)
            {
                item.Dispose();
            }

            foreach (Texture item in texture1)
            {
                item.Dispose();
            }

            foreach (Texture item in texture2)
            {
                item.Dispose();
            }

            foreach (Sampler item in samplerState)
            {
                item.Dispose();
            }
        });
    }
}
