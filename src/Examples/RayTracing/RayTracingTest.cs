using Common;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
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
    private BottomLevelAS bottomLevelAS = null!;
    private TopLevelAS topLevelAS = null!;
    private Texture output = null!;
    private ResourceLayout resourceLayout = null!;
    private ResourceSet resourceSet = null!;
    private RayTracingPipeline rayTracingPipeline = null!;

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

        ReflectResult reflectResult = SpvReflector.Reflect(rayGen);
        reflectResult = ReflectResult.Merge(reflectResult, SpvReflector.Reflect(miss));
        reflectResult = ReflectResult.Merge(reflectResult, SpvReflector.Reflect(closestHit));

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

        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        BottomLevelASDesc blasDesc = BottomLevelASDesc.Default(new AccelerationStructureTriangles(vertexBuffer)
        {
            VertexFormat = PixelFormat.R32G32B32Float,
            VertexStrideInBytes = (uint)sizeof(Vertex),
            VertexCount = (uint)vertices.Length,
            VertexOffsetInBytes = 0,
            IndexBuffer = indexBuffer,
            IndexFormat = IndexFormat.UInt32,
            IndexCount = (uint)indices.Length,
            IndexOffsetInBytes = 0,
            Transform = Matrix4X4<float>.Identity
        });

        bottomLevelAS = commandBuffer.BuildAccelerationStructure(in blasDesc);

        TopLevelASDesc tlasDesc = TopLevelASDesc.Default([new AccelerationStructureInstance(bottomLevelAS)
        {
            Transform = Matrix4X4<float>.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelerationStructureInstanceOptions.None
        }]);

        topLevelAS = commandBuffer.BuildAccelerationStructure(in tlasDesc);

        commandBuffer.End();

        commandBuffer.Commit();

        TextureDesc outputDesc = TextureDesc.Default(Width, Height, 1, 1, usage: TextureUsage.Sampled | TextureUsage.Storage);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceLayoutDesc rlDesc = ResourceLayoutDesc.Default(reflectResult["Scene", ShaderStages.RayGeneration].Desc,
                                                               reflectResult["VertexBuffer", ShaderStages.ClosestHit].Desc,
                                                               reflectResult["IndexBuffer", ShaderStages.ClosestHit].Desc,
                                                               reflectResult["Output", ShaderStages.RayGeneration].Desc);

        resourceLayout = Context.Factory.CreateResourceLayout(in rlDesc);

        ResourceSetDesc rsDesc = ResourceSetDesc.Default(resourceLayout, topLevelAS, vertexBuffer, indexBuffer, output);

        resourceSet = Context.Factory.CreateResourceSet(in rsDesc);

        ShaderDesc rgDesc = ShaderDesc.Default(ShaderStages.RayGeneration, rayGen, "RayGenMain");
        ShaderDesc msDesc = ShaderDesc.Default(ShaderStages.Miss, miss, "MissMain");
        ShaderDesc chDesc = ShaderDesc.Default(ShaderStages.ClosestHit, closestHit, "ClosestHitMain");

        using Shader rgShader = Context.Factory.CreateShader(in rgDesc);
        using Shader msShader = Context.Factory.CreateShader(in msDesc);
        using Shader chShader = Context.Factory.CreateShader(in chDesc);

        RayTracingPipelineDesc rtpDesc = RayTracingPipelineDesc.Default
        (
            shaders: RayTracingShaderDesc.Default(rgShader, [msShader], [chShader]),
            hitGroups: [HitGroupDesc.Default(closestHit: "ClosestHitMain")],
            resourceLayouts: [resourceLayout]
        );

        rayTracingPipeline = Context.Factory.CreateRayTracingPipeline(in rtpDesc);
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
        ImGui.GetBackgroundDrawList().AddImage(ImGuiController.GetBinding(output), new(0, 0), new(Width, Height));
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        CommandBuffer commandBuffer = CommandProcessor.CommandBuffer();

        commandBuffer.Begin();

        commandBuffer.PrepareResources(resourceSet);

        commandBuffer.SetRayTracingPipeline(rayTracingPipeline);
        commandBuffer.SetResourceSet(0, resourceSet);

        commandBuffer.DispatchRays(Width, Height, 1);

        commandBuffer.End();
        commandBuffer.Commit();
    }

    protected override void OnSizeChanged(uint width, uint height)
    {
        resourceSet.Dispose();
        output.Dispose();

        ImGuiController.RemoveBinding(output);

        TextureDesc outputDesc = TextureDesc.Default(width, height, 1, 1, usage: TextureUsage.Sampled | TextureUsage.Storage);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceSetDesc rsDesc = ResourceSetDesc.Default(resourceLayout, topLevelAS, vertexBuffer, indexBuffer, output);

        resourceSet = Context.Factory.CreateResourceSet(in rsDesc);
    }

    protected override void OnDestroy()
    {
        rayTracingPipeline.Dispose();
        resourceSet.Dispose();
        resourceLayout.Dispose();
        output.Dispose();
        topLevelAS.Dispose();
        bottomLevelAS.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }
}
