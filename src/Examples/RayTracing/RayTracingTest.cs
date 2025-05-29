using Common;
using Hexa.NET.ImGui;
using RayTracing.Models;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;

namespace RayTracing;

internal unsafe class RayTracingTest() : VisualTest("RayTracing Test")
{
    private readonly string shaderPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders");

    private BottomLevelAS blas = null!;
    private Uniforms uniforms = null!;
    private ResourceLayout layout = null!;
    private ResourceSet set = null!;
    private RayTracingPipeline pipeline = null!;

    private int cameraHash;

    protected override void OnLoad()
    {
        string rayGeneration = Path.Combine(shaderPath, "RayGen.slang");
        string miss = Path.Combine(shaderPath, "Miss.slang");
        string closestHit = Path.Combine(shaderPath, "ClosestHit.slang");

        using Shader rg = Context.Factory.CompileShader(rayGeneration, ShaderStages.RayGeneration, "RayGeneration", out ShaderReflection r1);
        using Shader ms = Context.Factory.CompileShader(miss, ShaderStages.Miss, "Miss", out ShaderReflection r2);
        using Shader sms = Context.Factory.CompileShader(miss, ShaderStages.Miss, "ShadowMiss", out ShaderReflection r3);
        using Shader ch = Context.Factory.CompileShader(closestHit, ShaderStages.ClosestHit, "ClosestHit", out ShaderReflection r4);
        ShaderReflection reflection = ShaderReflection.Merge(r1, r2, r3, r4);

        List<Vertex> allVertices = [];
        List<uint> allIndices = [];
        List<Vector2D<uint>> offsets = [];
        List<Material> materials = [];
        for (uint i = 0; i < 4; i++)
        {
            offsets.Add(new Vector2D<uint>((uint)allVertices.Count, (uint)allIndices.Count));

            Vertex.CornellBox(i, out Vertex[] vertices, out uint[] indices, out Material material);

            allVertices.AddRange(vertices);
            allIndices.AddRange(indices);
            materials.Add(material);
        }

        Buffer<Vertex> vertexBuffer = new(Context, (uint)allVertices.Count, BufferUsage.ShaderResource);
        vertexBuffer.CopyFrom(allVertices.ToArray());

        Buffer<uint> indexBuffer = new(Context, (uint)allIndices.Count, BufferUsage.ShaderResource);
        indexBuffer.CopyFrom(allIndices.ToArray());

        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        List<AccelerationStructureTriangles> triangles = [];
        for (int i = 0; i < offsets.Count; i++)
        {
            Vector2D<uint> offset = offsets[i];

            uint vertexCount;
            uint indexCount;
            if (i < offsets.Count - 1)
            {
                Vector2D<uint> next = offsets[i + 1];

                vertexCount = next.X - offset.X;
                indexCount = next.Y - offset.Y;
            }
            else
            {
                vertexCount = (uint)allVertices.Count - offset.X;
                indexCount = (uint)allIndices.Count - offset.Y;
            }

            triangles.Add(new(vertexBuffer)
            {
                VertexFormat = PixelFormat.R32G32B32Float,
                VertexStrideInBytes = (uint)sizeof(Vertex),
                VertexCount = vertexCount,
                VertexOffsetInBytes = offset.X * (uint)sizeof(Vertex),
                IndexBuffer = indexBuffer,
                IndexFormat = IndexFormat.UInt32,
                IndexCount = indexCount,
                IndexOffsetInBytes = offset.Y * sizeof(uint),
                Transform = Matrix3X4<float>.Identity,
                Options = AccelerationStructureGeometryOptions.Opaque
            });
        }

        BottomLevelASDesc bottomLevelASDesc = new([.. triangles]);

        blas = commandBuffer.BuildAccelerationStructure(in bottomLevelASDesc);

        TopLevelASDesc tlasDesc = new([new(blas)
        {
            Transform = Matrix3X4<float>.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0
        }]);

        TopLevelAS tlas = commandBuffer.BuildAccelerationStructure(in tlasDesc);

        commandBuffer.End();
        commandBuffer.Commit();

        Light light = new()
        {
            Type = LightType.Area,
            Position = new Vector3D<float>(340.0f, 548.0f, 230.0f),
            Emission = new Vector3D<float>(170.0f, 120.0f, 40.0f),
            U = new Vector3D<float>(0.0f, 0.0f, 100.0f),
            V = new Vector3D<float>(-130.0f, 0.0f, 0.0f),
            Area = 100.0f * 100.0f,
            Radius = 0.0f
        };

        uniforms = new(Context, tlas, [.. materials], vertexBuffer, indexBuffer, [.. offsets], [light], Width, Height);

        ResourceLayoutDesc layoutDesc = new
        (
            reflection["uniforms.Scene"].Desc,
            reflection["uniforms.Globals"].Desc,
            reflection["uniforms.Materials"].Desc,
            reflection["uniforms.Vertices"].Desc,
            reflection["uniforms.Indices"].Desc,
            reflection["uniforms.Offsets"].Desc,
            reflection["uniforms.Lights"].Desc,
            reflection["uniforms.Accumulation"].Desc,
            reflection["uniforms.Output"].Desc
        );

        layout = Context.Factory.CreateResourceLayout(in layoutDesc);

        ResourceSetDesc setDesc = new(layout,
                                      uniforms.Scene,
                                      uniforms.Globals,
                                      uniforms.Materials,
                                      uniforms.Vertices,
                                      uniforms.Indices,
                                      uniforms.Offsets,
                                      uniforms.Lights,
                                      uniforms.Accumulation,
                                      uniforms.Output);

        set = Context.Factory.CreateResourceSet(in setDesc);

        RayTracingPipelineDesc pipelineDesc = new()
        {
            Shaders = new(rg, [ms, sms], [ch]),
            HitGroups =
            [
                new("Default", closestHit: "ClosestHit"),
                new("Shadow")
            ],
            ResourceLayouts = [layout]
        };

        pipeline = Context.Factory.CreateRayTracingPipeline(in pipelineDesc);

        CameraController.Transform(Matrix4X4.CreateTranslation(278.000f, 273.000f, -800.000f));
        CameraController.Speed = 240.000f;
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
        ref Globals globals = ref uniforms.Globals[0];
        globals.Camera.Position = CameraController.Position;
        globals.Camera.Forward = CameraController.Forward;
        globals.Camera.Right = CameraController.Right;
        globals.Camera.Up = CameraController.Up;
        globals.Camera.Fov = Utils.DegreesToRadians(CameraController.Fov);
        globals.DoubleSidedLighting = true;
        globals.SampleCount = 2;
        globals.MaxDepth = 4;

        if (cameraHash != globals.Camera.GetHashCode())
        {
            globals.FrameIndex = 0;

            cameraHash = globals.Camera.GetHashCode();
        }

        ImGui.GetBackgroundDrawList().AddImage(ImGuiController.GetBinding(uniforms.Output), new(0, 0), new(Width, Height));
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.PrepareResources([set]);

        commandBuffer.SetRayTracingPipeline(pipeline);
        commandBuffer.SetResourceSet(0, set);

        commandBuffer.DispatchRays(Width, Height, 1);

        commandBuffer.End();
        commandBuffer.Commit();

        uniforms.Globals[0].FrameIndex++;
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
        set.Dispose();

        ImGuiController.RemoveBinding(uniforms.Output);

        uniforms.ResetTextures(width, height);

        ResourceSetDesc setDesc = new(layout,
                                      uniforms.Scene,
                                      uniforms.Globals,
                                      uniforms.Materials,
                                      uniforms.Vertices,
                                      uniforms.Indices,
                                      uniforms.Offsets,
                                      uniforms.Lights,
                                      uniforms.Accumulation,
                                      uniforms.Output);

        set = Context.Factory.CreateResourceSet(in setDesc);

        uniforms.Globals[0].FrameIndex = 0;
    }

    protected override void OnDestroy()
    {
        pipeline.Dispose();
        set.Dispose();
        layout.Dispose();
        uniforms.Dispose();
        blas.Dispose();
    }
}
