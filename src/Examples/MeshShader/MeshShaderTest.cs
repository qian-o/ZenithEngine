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
    private struct Vertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoord)
    {
        public Vector3D<float> Position = position;

        public Vector3D<float> Normal = normal;

        public Vector2D<float> TexCoord = texCoord;
    }

    private struct Meshlet
    {
        public uint PrimitiveCount;

        public fixed uint Vertices[64];

        public uint VertexCount;

        public fixed uint Indices[126];

        public uint IndexCount;
    };

    private Buffer verticesBuffer = null!;
    private Buffer meshletsBuffer = null!;
    private ResourceLayout layout = null!;
    private ResourceSet set = null!;
    private GraphicsPipeline pipeline = null!;

    protected override void OnLoad()
    {
        string shader = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.slang");

        Vertex[] vertices =
        [
            new(new(0.0f, 0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(0.5f, 1.0f)),
            new(new(0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(1.0f, 0.0f)),
            new(new(-0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(0.0f, 0.0f))
        ];

        uint[] indices = [0, 1, 2];

        Meshlet meshlet = new()
        {
            PrimitiveCount = 1,
            VertexCount = 3,
            IndexCount = 3
        };

        meshlet.Vertices[0] = 0;
        meshlet.Vertices[1] = 1;
        meshlet.Vertices[2] = 2;

        meshlet.Indices[0] = 0;
        meshlet.Indices[1] = 1;
        meshlet.Indices[2] = 2;

        Meshlet[] meshlets = [meshlet];

        BufferDesc verticesDesc = new((uint)(vertices.Length * sizeof(Vertex)), BufferUsage.ShaderResource);
        verticesBuffer = Context.Factory.CreateBuffer(in verticesDesc);

        BufferDesc meshletsDesc = new((uint)(meshlets.Length * sizeof(Meshlet)), BufferUsage.ShaderResource);
        meshletsBuffer = Context.Factory.CreateBuffer(in meshletsDesc);

        fixed (Vertex* pVertices = vertices)
        {
            Context.UpdateBuffer(verticesBuffer, (nint)pVertices, (uint)(vertices.Length * sizeof(Vertex)));
        }

        fixed (Meshlet* pMeshlets = meshlets)
        {
            Context.UpdateBuffer(meshletsBuffer, (nint)pMeshlets, (uint)(meshlets.Length * sizeof(Meshlet)));
        }

        using Shader msShader = Context.Factory.CompileShader(shader, ShaderStages.Mesh, "MeshMain", out ShaderReflection msReflection);
        using Shader psShader = Context.Factory.CompileShader(shader, ShaderStages.Pixel, "PixelMain", out ShaderReflection psReflection);
        ShaderReflection reflection = ShaderReflection.Merge(msReflection, psReflection);

    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
    }

    protected override void OnDestroy()
    {
    }
}
