using System.Runtime.InteropServices;
using Common;
using Common.Helpers;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;
using ResourceSet = ZenithEngine.Common.Graphics.ResourceSet;

namespace RayTracing;

internal unsafe class RayTracingTest(Backend backend) : VisualTest("RayTracing Test", backend)
{
    [StructLayout(LayoutKind.Explicit)]
    struct Camera
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
    };

    private readonly List<Buffer> vertexBuffers = [];
    private readonly List<Buffer> indexBuffers = [];

    private BottomLevelAS? blas;
    private TopLevelAS? tlas;
    private Buffer? cameraBuffer;
    private Texture? output;
    private ResourceLayout? resLayout;
    private ResourceSet? resSet;
    private RayTracingPipeline rtPipeline = null!;

    protected override void OnLoad()
    {
        string hlsl = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.hlsl"));

        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        List<AccelerationStructureTriangles> triangles = [];

        for (uint i = 0; i < 4; i++)
        {
            Vertex.CornellBox(i, out Vertex[] vertices, out uint[] indices);

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
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelerationStructureInstanceOptions.None
        }]);

        tlas = commandBuffer.BuildAccelerationStructure(in tlasDesc);

        commandBuffer.End();
        commandBuffer.Commit();

        BufferDesc cameraBufferDesc = new((uint)sizeof(Camera), BufferUsage.Dynamic | BufferUsage.ConstantBuffer);

        cameraBuffer = Context.Factory.CreateBuffer(in cameraBufferDesc);

        TextureDesc outputDesc = new(Width,
                                     Height,
                                     usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceLayoutDesc resLayoutDesc = new
        ([
            new(ShaderStages.RayGeneration, ResourceType.AccelerationStructure, 0),
            new(ShaderStages.ClosestHit, ResourceType.StructuredBuffer, 1, 4),
            new(ShaderStages.ClosestHit, ResourceType.StructuredBuffer, 5, 4),
            new(ShaderStages.RayGeneration, ResourceType.ConstantBuffer, 0),
            new(ShaderStages.RayGeneration, ResourceType.TextureReadWrite, 0),
        ]);

        resLayout = Context.Factory.CreateResourceLayout(in resLayoutDesc);

        ResourceSetDesc resSetDesc = new(resLayout, [tlas, .. vertexBuffers, .. indexBuffers, cameraBuffer, output]);

        resSet = Context.Factory.CreateResourceSet(in resSetDesc);

        using Shader rgShader = Context.Factory.CompileShader(ShaderStages.RayGeneration, hlsl, "RayGenMain");
        using Shader msShader = Context.Factory.CompileShader(ShaderStages.Miss, hlsl, "MissMain");
        using Shader chShader = Context.Factory.CompileShader(ShaderStages.ClosestHit, hlsl, "ClosestHitMain");

        RayTracingPipelineDesc rtpDesc = new
        (
            shaders: new(rgShader, [msShader], [chShader]),
            hitGroups: [new("DefaultHitGroup", closestHit: "ClosestHitMain")],
            resourceLayouts: [resLayout]
        );

        rtPipeline = Context.Factory.CreateRayTracingPipeline(in rtpDesc);

        CameraController.Transform(Matrix4X4.CreateTranslation(278.000f, 274.400f, -800.000f));
        CameraController.FarPlane = 2000.000f;
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
        Camera camera = new()
        {
            Position = CameraController.Position,
            Forward = CameraController.Forward,
            Right = CameraController.Right,
            Up = CameraController.Up,
            NearPlane = CameraController.NearPlane,
            FarPlane = CameraController.FarPlane,
            Fov = CameraController.Fov.ToRadians()
        };

        Context.UpdateBuffer(cameraBuffer!, (nint)(&camera), (uint)sizeof(Camera));

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

        ResourceSetDesc resSetDesc = new(resLayout!, [tlas!, .. vertexBuffers, .. indexBuffers, cameraBuffer!, output]);

        resSet = Context.Factory.CreateResourceSet(in resSetDesc);
    }

    protected override void OnDestroy()
    {
        rtPipeline.Dispose();
        resSet?.Dispose();
        resLayout?.Dispose();
        output?.Dispose();
        cameraBuffer?.Dispose();
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
