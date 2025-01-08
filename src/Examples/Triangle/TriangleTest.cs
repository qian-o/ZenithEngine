using Common;
using Hexa.NET.ImGui;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace Triangle;

internal unsafe class TriangleTest(Backend backend) : VisualTest("Triangle Test", backend)
{
    private Buffer vertexBuffer = null!;
    private Buffer indexBuffer = null!;
    private GraphicsPipeline pipeline = null!;

    protected override void OnLoad()
    {
        Vertex[] vertices =
        [
            new(new(0.0f, 0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(0.5f, 1.0f)),
            new(new(0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(1.0f, 0.0f)),
            new(new(-0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(0.0f, 0.0f))
        ];

        uint[] indices = [0, 1, 2];

        string hlsl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.hlsl"));

        byte[] vs = DxcCompiler.Compile(ShaderStages.Vertex, hlsl, "VertexMain");
        byte[] ps = DxcCompiler.Compile(ShaderStages.Pixel, hlsl, "PixelMain");

        BufferDesc vbDesc = BufferDesc.Default((uint)(vertices.Length * sizeof(Vertex)), BufferUsage.VertexBuffer);

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);

        BufferDesc ibDesc = BufferDesc.Default((uint)(indices.Length * sizeof(uint)), BufferUsage.IndexBuffer);

        indexBuffer = Context.Factory.CreateBuffer(in ibDesc);

        fixed (Vertex* pVertices = vertices)
        {
            Context.UpdateBuffer(vertexBuffer, (nint)pVertices, (uint)(vertices.Length * sizeof(Vertex)));
        }

        fixed (uint* pIndices = indices)
        {
            Context.UpdateBuffer(indexBuffer, (nint)pIndices, (uint)(indices.Length * sizeof(uint)));
        }

        ShaderDesc vsDesc = ShaderDesc.Default(ShaderStages.Vertex, vs, "VertexMain");
        ShaderDesc psDesc = ShaderDesc.Default(ShaderStages.Pixel, ps, "PixelMain");

        using Shader vsShader = Context.Factory.CreateShader(in vsDesc);
        using Shader psShader = Context.Factory.CreateShader(in psDesc);

        GraphicsPipelineDesc gpDesc = GraphicsPipelineDesc.Default
        (
            shaders: GraphicsShaderDesc.Default(vertex: vsShader, pixel: psShader),
            inputLayouts: [Vertex.GetLayout()],
            resourceLayouts: [],
            outputs: SwapChain.FrameBuffer.Output,
            renderStates: RenderStateDesc.Default(RasterizerStates.None, DepthStencilStates.None, BlendStates.Opaque)
        );

        pipeline = Context.Factory.CreateGraphicsPipeline(in gpDesc);
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
        ImGui.ShowDemoWindow();
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.BeginRendering(SwapChain.FrameBuffer, new(1));

        commandBuffer.SetGraphicsPipeline(pipeline);

        commandBuffer.SetVertexBuffer(0, vertexBuffer);
        commandBuffer.SetIndexBuffer(indexBuffer, IndexFormat.U32Bit);

        commandBuffer.DrawIndexed(3);

        commandBuffer.EndRendering();

        commandBuffer.End();

        commandBuffer.Commit();
    }

    protected override void OnDestroy()
    {
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        pipeline.Dispose();
    }
}
