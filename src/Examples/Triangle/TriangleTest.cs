using System.Runtime.InteropServices;
using Common;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace Triangle;

internal unsafe class TriangleTest(Backend backend) : VisualTest("Triangle Test", backend)
{
    [StructLayout(LayoutKind.Explicit)]
    private struct Constants
    {
        [FieldOffset(0)]
        public Matrix4X4<float> Model;
    }

    private Buffer vertexBuffer = null!;
    private Buffer indexBuffer = null!;
    private Buffer constantsBuffer = null!;
    private ResourceLayout layout = null!;
    private ResourceSet set = null!;
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

        Constants constants = new()
        {
            Model = Matrix4X4<float>.Identity
        };

        string hlsl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.hlsl"));

        BufferDesc vbDesc = BufferDesc.New((uint)(vertices.Length * sizeof(Vertex)), BufferUsage.VertexBuffer);

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);

        BufferDesc ibDesc = BufferDesc.New((uint)(indices.Length * sizeof(uint)), BufferUsage.IndexBuffer);

        indexBuffer = Context.Factory.CreateBuffer(in ibDesc);

        BufferDesc cbDesc = BufferDesc.New((uint)sizeof(Constants), BufferUsage.ConstantBuffer);

        constantsBuffer = Context.Factory.CreateBuffer(in cbDesc);

        fixed (Vertex* pVertices = vertices)
        {
            Context.UpdateBuffer(vertexBuffer, (nint)pVertices, (uint)(vertices.Length * sizeof(Vertex)));
        }

        fixed (uint* pIndices = indices)
        {
            Context.UpdateBuffer(indexBuffer, (nint)pIndices, (uint)(indices.Length * sizeof(uint)));
        }

        Context.UpdateBuffer(constantsBuffer, (nint)(&constants), (uint)sizeof(Constants));

        using Shader vsShader = Context.Factory.CompileShader(ShaderStages.Vertex, hlsl, "VertexMain");
        using Shader psShader = Context.Factory.CompileShader(ShaderStages.Pixel, hlsl, "PixelMain");

        ResourceLayoutDesc layoutDesc = ResourceLayoutDesc.New
        (
            LayoutElementDesc.New(ShaderStages.Vertex, ResourceType.ConstantBuffer, 0)
        );

        layout = Context.Factory.CreateResourceLayout(in layoutDesc);

        ResourceSetDesc setDesc = ResourceSetDesc.New(layout, constantsBuffer);

        set = Context.Factory.CreateResourceSet(in setDesc);

        GraphicsPipelineDesc gpDesc = GraphicsPipelineDesc.New
        (
            shaders: GraphicsShaderDesc.New(vertex: vsShader, pixel: psShader),
            inputLayouts: [Vertex.GetLayout()],
            resourceLayouts: [layout],
            outputs: SwapChain.FrameBuffer.Output,
            renderStates: RenderStateDesc.New(RasterizerStates.None, DepthStencilStates.None, BlendStates.Opaque)
        );

        pipeline = Context.Factory.CreateGraphicsPipeline(in gpDesc);
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.BeginRendering(SwapChain.FrameBuffer, new(1));

        commandBuffer.SetGraphicsPipeline(pipeline);

        commandBuffer.SetVertexBuffer(0, vertexBuffer);
        commandBuffer.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);
        commandBuffer.SetResourceSet(0, set);

        commandBuffer.DrawIndexed(3, 1);

        commandBuffer.EndRendering();

        commandBuffer.End();

        commandBuffer.Commit();
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
    }

    protected override void OnDestroy()
    {
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        constantsBuffer.Dispose();
        layout.Dispose();
        set.Dispose();
        pipeline.Dispose();
    }
}
