﻿using Common;
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

        BufferDesc vbDesc = new((uint)(vertices.Length * sizeof(Vertex)),
                                BufferUsage.ShaderResource | BufferUsage.AccelerationStructure,
                                (uint)sizeof(Vertex));

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);

        BufferDesc ibDesc = new((uint)(indices.Length * sizeof(uint)),
                                BufferUsage.ShaderResource | BufferUsage.AccelerationStructure,
                                sizeof(uint));

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

        BottomLevelASDesc blasDesc = new(new AccelerationStructureTriangles(vertexBuffer)
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

        bottomLevelAS = commandBuffer.BuildAccelerationStructure(in blasDesc);

        TopLevelASDesc tlasDesc = new([new(bottomLevelAS)
        {
            Transform = Matrix3X4<float>.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelerationStructureInstanceOptions.None
        }]);

        topLevelAS = commandBuffer.BuildAccelerationStructure(in tlasDesc);

        commandBuffer.End();

        commandBuffer.Commit();

        TextureDesc outputDesc = new(Width, Height, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceLayoutDesc rlDesc = new
        (
            new(ShaderStages.RayGeneration, ResourceType.AccelerationStructure, 0),
            new(ShaderStages.ClosestHit, ResourceType.StructuredBuffer, 1),
            new(ShaderStages.ClosestHit, ResourceType.StructuredBuffer, 2),
            new(ShaderStages.RayGeneration, ResourceType.TextureReadWrite, 0)
        );

        resourceLayout = Context.Factory.CreateResourceLayout(in rlDesc);

        ResourceSetDesc rsDesc = new(resourceLayout, topLevelAS, vertexBuffer, indexBuffer, output);

        resourceSet = Context.Factory.CreateResourceSet(in rsDesc);

        using Shader rgShader = Context.Factory.CompileShader(ShaderStages.RayGeneration, hlsl, "RayGenMain");
        using Shader msShader = Context.Factory.CompileShader(ShaderStages.Miss, hlsl, "MissMain");
        using Shader chShader = Context.Factory.CompileShader(ShaderStages.ClosestHit, hlsl, "ClosestHitMain");

        RayTracingPipelineDesc rtpDesc = new
        (
            shaders: new(rgShader, [msShader], [chShader]),
            hitGroups: [new("DefaultHitGroup", closestHit: "ClosestHitMain")],
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

        commandBuffer.PrepareResources([resourceSet]);

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

        TextureDesc outputDesc = new(width, height, usage: TextureUsage.ShaderResource | TextureUsage.UnorderedAccess);

        output = Context.Factory.CreateTexture(in outputDesc);

        ResourceSetDesc rsDesc = new(resourceLayout, topLevelAS, vertexBuffer, indexBuffer, output);

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
