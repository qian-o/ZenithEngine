﻿using System.Runtime.InteropServices;
using Common;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ShaderCompiler;
using Buffer = ZenithEngine.Common.Graphics.Buffer;

namespace Triangle;

internal unsafe class TriangleTest() : VisualTest("Triangle Test")
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
            new(new(0.0f, 0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(0.5f, 1.0f), -1),
            new(new(0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(1.0f, 0.0f), -1),
            new(new(-0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 1.0f), new(0.0f, 0.0f), -1)
        ];

        uint[] indices = [0, 1, 2];

        Constants constants = new()
        {
            Model = Matrix4X4<float>.Identity
        };

        string shader = Path.Combine(AppContext.BaseDirectory, "Assets", "Shaders", "Shader.slang");

        BufferDesc vbDesc = new((uint)(vertices.Length * sizeof(Vertex)), BufferUsage.VertexBuffer);

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);

        BufferDesc ibDesc = new((uint)(indices.Length * sizeof(uint)), BufferUsage.IndexBuffer);

        indexBuffer = Context.Factory.CreateBuffer(in ibDesc);

        BufferDesc cbDesc = new((uint)sizeof(Constants), BufferUsage.ConstantBuffer);

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

        using Shader vsShader = Context.Factory.CompileShader(shader, ShaderStages.Vertex, "VertexMain", out ShaderReflection vsReflection);
        using Shader psShader = Context.Factory.CompileShader(shader, ShaderStages.Pixel, "PixelMain", out ShaderReflection psReflection);
        ShaderReflection reflection = ShaderReflection.Merge(vsReflection, psReflection);

        ResourceLayoutDesc layoutDesc = new(reflection["constants"].Desc);

        layout = Context.Factory.CreateResourceLayout(in layoutDesc);

        ResourceSetDesc setDesc = new(layout, constantsBuffer);

        set = Context.Factory.CreateResourceSet(in setDesc);

        GraphicsPipelineDesc gpDesc = new
        (
            shaders: new(vertex: vsShader, pixel: psShader),
            inputLayouts: [Vertex.GetInputLayout()],
            resourceLayouts: [layout],
            outputs: SwapChain.FrameBuffer.Output,
            renderStates: new(RasterizerStates.None, DepthStencilStates.None, BlendStates.Opaque)
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
