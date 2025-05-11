using System.Runtime.InteropServices;
using Common;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;
using ResourceSet = ZenithEngine.Common.Graphics.ResourceSet;

namespace RayTracing;

internal unsafe class RayTracingTest() : VisualTest("RayTracing Test")
{
    [StructLayout(LayoutKind.Explicit)]
    private struct Camera
    {
        [FieldOffset(0)]
        public Vector3D<float> Position;

        [FieldOffset(16)]
        public Vector3D<float> Forward;

        [FieldOffset(32)]
        public Vector3D<float> Right;

        [FieldOffset(48)]
        public Vector3D<float> Up;

        [FieldOffset(60)]
        public float NearPlane;

        [FieldOffset(64)]
        public float FarPlane;

        [FieldOffset(68)]
        public float Fov;

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, Forward, Right, Up, NearPlane, FarPlane, Fov);
        }
    };

    [StructLayout(LayoutKind.Explicit)]
    private struct AO
    {
        [FieldOffset(0)]
        public float Radius;

        [FieldOffset(4)]
        public int Samples;

        [FieldOffset(8)]
        public float Power;

        [FieldOffset(12)]
        public bool DistanceBased;

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Radius, Samples, Power, DistanceBased);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct Global
    {
        [FieldOffset(0)]
        public Camera Camera;

        [FieldOffset(80)]
        public AO AO;

        [FieldOffset(96)]
        public int FrameNumber;

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Camera, AO);
        }
    }

    private readonly List<Buffer> vertexBuffers = [];
    private readonly List<Buffer> indexBuffers = [];

    private BottomLevelAS? blas;
    private TopLevelAS? tlas;
    private Buffer? materialsBuffer;
    private Buffer? globalBuffer;
    private Texture? output;
    private ResourceLayout? resLayout;
    private ResourceSet? resSet;
    private RayTracingPipeline rtPipeline = null!;

    private Global global;
    private int globalHash;

    protected override void OnLoad()
    {
        string shader = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.slang");

        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        List<AccelerationStructureTriangles> triangles = [];

        Material[] materials = new Material[4];

        for (uint i = 0; i < 4; i++)
        {
            Vertex.CornellBox(i,
                              out Vertex[] vertices,
                              out uint[] indices,
                              out materials[i]);

            BufferDesc vertexBufferDesc = new((uint)(vertices.Length * sizeof(Vertex)),
                                              BufferUsage.ShaderResource | BufferUsage.AccelerationStructure,
                                              (uint)sizeof(Vertex));

            Buffer vertexBuffer = Context.Factory.CreateBuffer(in vertexBufferDesc);

            fixed (void* pVertices = vertices)
            {
                Context.UpdateBuffer(vertexBuffer, (nint)pVertices, vertexBufferDesc.SizeInBytes);
            }

            BufferDesc indexBufferDesc = new((uint)(indices.Length * sizeof(uint)),
                                             BufferUsage.ShaderResource | BufferUsage.AccelerationStructure,
                                             sizeof(uint));

            Buffer indexBuffer = Context.Factory.CreateBuffer(in indexBufferDesc);

            fixed (void* pIndices = indices)
            {
                Context.UpdateBuffer(indexBuffer, (nint)pIndices, indexBufferDesc.SizeInBytes);
            }

            vertexBuffers.Add(vertexBuffer);
            indexBuffers.Add(indexBuffer);

            triangles.Add(new(vertexBuffer)
            {
                VertexFormat = PixelFormat.R32G32B32Float,
                VertexStrideInBytes = (uint)sizeof(Vertex),
                VertexCount = (uint)vertices.Length,
                VertexOffsetInBytes = 0,
                IndexBuffer = indexBuffer,
                IndexFormat = IndexFormat.UInt32,
                IndexCount = (uint)indices.Length,
                IndexOffsetInBytes = 0,
                Transform = Matrix3X4<float>.Identity
            });
        }

        BottomLevelASDesc blasDesc = new([.. triangles]);

        blas = commandBuffer.BuildAccelerationStructure(in blasDesc);

        TopLevelASDesc tlasDesc = new([new(blas)
        {
            Transform = Matrix3X4<float>.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0
        }]);

        tlas = commandBuffer.BuildAccelerationStructure(in tlasDesc);

        commandBuffer.End();
        commandBuffer.Commit();

        BufferDesc materialsBufferDesc = new((uint)(materials.Length * sizeof(Material)),
                                             BufferUsage.ShaderResource,
                                             (uint)sizeof(Material));

        materialsBuffer = Context.Factory.CreateBuffer(in materialsBufferDesc);

        fixed (void* pMaterials = materials)
        {
            Context.UpdateBuffer(materialsBuffer, (nint)pMaterials, materialsBufferDesc.SizeInBytes);
        }

        BufferDesc globalBufferDesc = new((uint)sizeof(Global), BufferUsage.Dynamic | BufferUsage.ConstantBuffer);

        globalBuffer = Context.Factory.CreateBuffer(in globalBufferDesc);

        TextureDesc outputDesc = new(Width,
                                     Height,
                                     usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ShaderReflection reflection = ShaderReflection.Empty;
        using Shader rgShader = Context.Factory.CompileShader(shader, ShaderStages.RayGeneration, "RayGenMain", ref reflection);
        using Shader msShader = Context.Factory.CompileShader(shader, ShaderStages.Miss, "MissMain", ref reflection);
        using Shader chShader = Context.Factory.CompileShader(shader, ShaderStages.ClosestHit, "ClosestHitMain", ref reflection);
        using Shader msAoShader = Context.Factory.CompileShader(shader, ShaderStages.Miss, "MissAO", ref reflection);
        using Shader chAoShader = Context.Factory.CompileShader(shader, ShaderStages.ClosestHit, "ClosestHitAO", ref reflection);

        ResourceLayoutDesc resLayoutDesc = new
        (
            reflection["scene"].Desc,
            reflection["vertexBuffers"].Desc,
            reflection["indexBuffers"].Desc,
            reflection["materials"].Desc,
            reflection["global"].Desc,
            reflection["output"].Desc
        );

        resLayout = Context.Factory.CreateResourceLayout(in resLayoutDesc);

        ResourceSetDesc resSetDesc = new(resLayout,
        [
            tlas,
            .. vertexBuffers,
            .. indexBuffers,
            materialsBuffer,
            globalBuffer,
            output
        ]);

        resSet = Context.Factory.CreateResourceSet(in resSetDesc);

        RayTracingPipelineDesc rtpDesc = new
        (
            shaders: new(rgShader, [msShader, msAoShader], [chShader, chAoShader]),
            hitGroups:
            [
                new("DefaultHitGroup", closestHit: "ClosestHitMain"),
                new("AoHitGroup", closestHit: "ClosestHitAO")
            ],
            resourceLayouts: [resLayout]
        );

        rtPipeline = Context.Factory.CreateRayTracingPipeline(in rtpDesc);

        CameraController.Transform(Matrix4X4.CreateTranslation(278.000f, 273.000f, -800.000f));
        CameraController.FarPlane = 2400.000f;
        CameraController.Speed = 240.000f;

        global = new()
        {
            AO = new()
            {
                Radius = 8.0f,
                Samples = 8,
                Power = 3.0f,
                DistanceBased = true
            }
        };
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
        global.Camera = new()
        {
            Position = CameraController.Position,
            Forward = CameraController.Forward,
            Right = CameraController.Right,
            Up = CameraController.Up,
            NearPlane = CameraController.NearPlane,
            FarPlane = CameraController.FarPlane,
            Fov = Utils.DegreesToRadians(CameraController.Fov)
        };

        if (ImGui.Begin("AO Settings"))
        {
            ImGui.SliderFloat("Radius", ref global.AO.Radius, 0.0f, 32.0f);
            ImGui.SliderInt("Samples", ref global.AO.Samples, 1, 64);
            ImGui.SliderFloat("Power", ref global.AO.Power, 0.0f, 10.0f);
            ImGui.Checkbox("Distance Based", ref global.AO.DistanceBased);
        }
        ImGui.End();

        global.FrameNumber++;

        int newGlobalHash = global.GetHashCode();

        if (globalHash != newGlobalHash)
        {
            global.FrameNumber = 0;

            globalHash = newGlobalHash;
        }

        fixed (void* globalPtr = &global)
        {
            Context.UpdateBuffer(globalBuffer!, (nint)globalPtr, (uint)sizeof(Global));
        }

        if (output is null)
        {
            return;
        }

        ImGui.GetBackgroundDrawList().AddImage(ImGuiController.GetBinding(output), new(0, 0), new(Width, Height));
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        if (output is null)
        {
            return;
        }

        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.PrepareResources([resSet!]);

        commandBuffer.SetRayTracingPipeline(rtPipeline);
        commandBuffer.SetResourceSet(0, resSet!);

        commandBuffer.DispatchRays(Width, Height, 1);

        commandBuffer.End();
        commandBuffer.Commit();
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
        if (output is not null)
        {
            ImGuiController.RemoveBinding(output);
        }

        resSet?.Dispose();
        output?.Dispose();

        TextureDesc outputDesc = new(width,
                                     height,
                                     usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceSetDesc resSetDesc = new(resLayout!,
        [
            tlas!,
            .. vertexBuffers,
            .. indexBuffers,
            materialsBuffer!,
            globalBuffer!,
            output
        ]);

        resSet = Context.Factory.CreateResourceSet(in resSetDesc);

        global.FrameNumber = 0;
    }

    protected override void OnDestroy()
    {
        rtPipeline.Dispose();
        resSet?.Dispose();
        resLayout?.Dispose();
        output?.Dispose();
        globalBuffer?.Dispose();
        materialsBuffer?.Dispose();
        tlas?.Dispose();
        blas?.Dispose();

        foreach (Buffer vertexBuffer in vertexBuffers)
        {
            vertexBuffer.Dispose();
        }

        foreach (Buffer indexBuffer in indexBuffers)
        {
            indexBuffer.Dispose();
        }
    }
}
