using Common;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace MeshShader;

internal unsafe class MeshShaderTest() : VisualTest("Mesh Shader Test")
{
    private struct Vertex(Vector4D<float> position, Vector3D<float> normal, Vector2D<float> texCoord)
    {
        public Vector4D<float> Position = position;

        public Vector3D<float> Normal = normal;

        public Vector2D<float> TexCoord = texCoord;
    }

    private struct Meshlet
    {
        public uint VertexOffset;

        public uint VertexCount;

        public uint PrimitiveOffset;

        public uint PrimitiveCount;
    };

    private Buffer verticesBuffer = null!;
    private Buffer indicesBuffer = null!;
    private Buffer meshletsBuffer = null!;
    private ResourceLayout layout = null!;
    private ResourceSet set = null!;
    private MeshShaderPipeline pipeline = null!;

    protected override void OnLoad()
    {
        string shader = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.slang");

        Vertex[] vertices =
        [
            new(new(0.0f, 0.5f, 0.0f, 1.0f), new(0.0f, 0.0f, 1.0f), new(0.5f, 1.0f)),
            new(new(0.5f, -0.5f, 0.0f, 1.0f), new(0.0f, 0.0f, 1.0f), new(1.0f, 0.0f)),
            new(new(-0.5f, -0.5f, 0.0f, 1.0f), new(0.0f, 0.0f, 1.0f), new(0.0f, 0.0f))
        ];

        uint[] indices = [0, 1, 2];

        Meshlet meshlet = new()
        {
            VertexOffset = 0,
            VertexCount = 3,
            PrimitiveOffset = 0,
            PrimitiveCount = 1
        };

        Meshlet[] meshlets = [meshlet];

        BufferDesc verticesDesc = new((uint)(vertices.Length * sizeof(Vertex)), BufferUsage.ShaderResource, (uint)sizeof(Vertex));
        verticesBuffer = Context.Factory.CreateBuffer(in verticesDesc);

        BufferDesc indicesDesc = new((uint)(indices.Length * sizeof(uint)), BufferUsage.ShaderResource, sizeof(uint) * 3);
        indicesBuffer = Context.Factory.CreateBuffer(in indicesDesc);

        BufferDesc meshletsDesc = new((uint)(meshlets.Length * sizeof(Meshlet)), BufferUsage.ShaderResource, (uint)sizeof(Meshlet));
        meshletsBuffer = Context.Factory.CreateBuffer(in meshletsDesc);

        fixed (Vertex* pVertices = vertices)
        {
            Context.UpdateBuffer(verticesBuffer, (nint)pVertices, (uint)(vertices.Length * sizeof(Vertex)));
        }

        fixed (uint* pIndices = indices)
        {
            Context.UpdateBuffer(indicesBuffer, (nint)pIndices, (uint)(indices.Length * sizeof(uint)));
        }

        fixed (Meshlet* pMeshlets = meshlets)
        {
            Context.UpdateBuffer(meshletsBuffer, (nint)pMeshlets, (uint)(meshlets.Length * sizeof(Meshlet)));
        }

        using Shader msShader = Context.Factory.CompileShader(shader, ShaderStages.Mesh, "MeshMain", out ShaderReflection msReflection);
        using Shader psShader = Context.Factory.CompileShader(shader, ShaderStages.Pixel, "PixelMain", out ShaderReflection psReflection);
        ShaderReflection reflection = ShaderReflection.Merge(msReflection, psReflection);

        ResourceLayoutDesc layoutDesc = new(reflection["vertices"].Desc,
                                            reflection["indices"].Desc,
                                            reflection["meshlets"].Desc);

        layout = Context.Factory.CreateResourceLayout(in layoutDesc);

        ResourceSetDesc setDesc = new(layout, verticesBuffer, indicesBuffer, meshletsBuffer);

        set = Context.Factory.CreateResourceSet(in setDesc);

        MeshShaderPipelineDesc mspDesc = new
        (
            shaders: new(mesh: msShader, pixel: psShader),
            resourceLayouts: [layout],
            outputs: SwapChain.FrameBuffer.Output,
            renderStates: new(RasterizerStates.None, DepthStencilStates.None, BlendStates.Opaque)
        );

        pipeline = Context.Factory.CreateMeshShaderPipeline(in mspDesc);
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.BeginRendering(SwapChain.FrameBuffer, new(1));

        commandBuffer.SetMeshShaderPipeline(pipeline);
        commandBuffer.SetResourceSet(0, set);

        commandBuffer.DispatchMesh(1, 1, 1);

        commandBuffer.EndRendering();

        commandBuffer.End();

        commandBuffer.Commit();
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
    }

    protected override void OnDestroy()
    {
        pipeline.Dispose();
        set.Dispose();
        layout.Dispose();
        meshletsBuffer.Dispose();
        indicesBuffer.Dispose();
        verticesBuffer.Dispose();
    }
}
