using Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace RayTracing;

internal unsafe class RayTracingTest(Backend backend) : VisualTest("RayTracing Test", backend)
{
    private Buffer vertexBuffer = null!;
    private Buffer indexBuffer = null!;
    private readonly BottomLevelAS bottomLevelAS = null!;
    private readonly TopLevelAS topLevelAS = null!;

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

        byte[] rayGen = DxcCompiler.Compile(ShaderStages.RayGeneration, hlsl, "RayGenMain");
        byte[] miss = DxcCompiler.Compile(ShaderStages.Miss, hlsl, "MissMain");
        byte[] closestHit = DxcCompiler.Compile(ShaderStages.ClosestHit, hlsl, "ClosestHitMain");
        
        BufferDesc vbDesc = BufferDesc.Default((uint)(vertices.Length * sizeof(Vertex)), BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure);

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);

        BufferDesc ibDesc = BufferDesc.Default((uint)(indices.Length * sizeof(uint)), BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure);

        indexBuffer = Context.Factory.CreateBuffer(in ibDesc);

        fixed (Vertex* pVertices = vertices)
        {
            Context.UpdateBuffer(vertexBuffer, (nint)pVertices, (uint)(vertices.Length * sizeof(Vertex)));
        }

        fixed (uint* pIndices = indices)
        {
            Context.UpdateBuffer(indexBuffer, (nint)pIndices, (uint)(indices.Length * sizeof(uint)));
        }

        BottomLevelASDesc blasDesc = BottomLevelASDesc.Default(new AccelerationStructureTriangles(vertexBuffer)
        {
            VertexFormat = PixelFormat.R32G32B32Float,
            VertexStrideInBytes = (uint)sizeof(Vertex),
            VertexCount = (uint)vertices.Length,
            VertexOffsetInBytes = 0,
            IndexBuffer = indexBuffer,
            IndexFormat = IndexFormat.UInt32,
        });
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
    }

    protected override void OnDestroy()
    {
    }
}
