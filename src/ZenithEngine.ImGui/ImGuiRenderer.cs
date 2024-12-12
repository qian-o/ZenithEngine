using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGui;

public unsafe class ImGuiRenderer : DisposableObject
{
    private Buffer vertexBuffer = null!;
    private Buffer indexBuffer = null!;
    private Buffer constantsBuffer = null!;
    private ResourceLayout layout0 = null!;
    private ResourceLayout layout1 = null!;
    private GraphicsPipeline pipeline = null!;

    public ImGuiRenderer(GraphicsContext context, OutputDesc outputDesc, ColorSpaceHandling handling)
    {
        Context = context;

        CreateGraphicsResources(outputDesc, handling);
    }

    public GraphicsContext Context { get; }

    protected override void Destroy()
    {
    }

    private void CreateGraphicsResources(OutputDesc outputDesc, ColorSpaceHandling handling)
    {
        BufferDesc vbDesc = BufferDesc.Default((uint)sizeof(ImDrawVert) * 5000,
                                               BufferUsage.VertexBuffer);

        vertexBuffer = Context.Factory.CreateBuffer(ref vbDesc);

        BufferDesc ibDesc = BufferDesc.Default(sizeof(ushort) * 10000,
                                               BufferUsage.IndexBuffer);

        indexBuffer = Context.Factory.CreateBuffer(ref ibDesc);

        BufferDesc cbDesc = BufferDesc.Default((uint)sizeof(Matrix4X4<float>),
                                               BufferUsage.ConstantBuffer);

        constantsBuffer = Context.Factory.CreateBuffer(ref cbDesc);

        ResourceLayoutDesc layout0Desc = ResourceLayoutDesc.Default(
        [
            LayoutElementDesc.Default(ShaderStages.Vertex, ResourceType.ConstantBuffer, 0),
            LayoutElementDesc.Default(ShaderStages.Pixel, ResourceType.Sampler, 0)
        ]);

        layout0 = Context.Factory.CreateResourceLayout(ref layout0Desc);

        ResourceLayoutDesc layout1Desc = ResourceLayoutDesc.Default(
        [
            LayoutElementDesc.Default(ShaderStages.Pixel, ResourceType.Texture, 0)
        ]);

        layout1 = Context.Factory.CreateResourceLayout(ref layout1Desc);

        Shaders.Get(handling, out byte[] vs, out byte[] ps);

        ShaderDesc shaderDesc = ShaderDesc.Default(ShaderStages.Vertex, vs, Shaders.VSMain);

        using Shader vsShader = Context.Factory.CreateShader(ref shaderDesc);

        shaderDesc = ShaderDesc.Default(ShaderStages.Pixel, ps, Shaders.PSMain);

        using Shader psShader = Context.Factory.CreateShader(ref shaderDesc);

        LayoutDesc layoutDesc = LayoutDesc.Default();
        layoutDesc.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.Position, 0));
        layoutDesc.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.Normal, 0));
        layoutDesc.Add(ElementDesc.Default(ElementFormat.Float4, ElementSemanticType.Color, 0));

        RenderStateDesc renderStateDesc = RenderStateDesc.Default();
        renderStateDesc.RasterizerState = RasterizerStateDesc.Default(CullMode.None);
        renderStateDesc.DepthStencilState = DepthStencilStateDesc.Default(false);

        GraphicsPipelineDesc pipelineDesc = GraphicsPipelineDesc.Default
        (
            GraphicsShaderDesc.Default(vertex: vsShader, pixel: psShader),
            [layoutDesc],
            [layout0, layout1],
            outputDesc,
            renderStateDesc
        );

        pipeline = Context.Factory.CreateGraphicsPipeline(ref pipelineDesc);
    }
}
