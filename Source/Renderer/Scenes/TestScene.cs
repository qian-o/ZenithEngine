﻿using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Components;

namespace Renderer.Scenes;

internal sealed unsafe class TestScene : Scene
{
    #region Structs
    private struct Vertex(Vector2 position, Vector4 color)
    {
        public const uint SizeInBytes = 24;

        public Vector2 Position = position;

        public Vector4 Color = color;
    }

    private struct Ubo
    {
        public Vector4 Value;
    }
    #endregion

    private const string HLSL = @"
struct UBO
{
    float4 Value;
};

struct VSInput
{
    [[vk::location(0)]] float2 Position : POSITION0;
    [[vk::location(1)]] float4 Color : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    [[vk::location(0)]] float4 Color : COLOR0;
};

ConstantBuffer<UBO> begin : register(b0, space0);
ConstantBuffer<UBO> end : register(b1, space0);
ConstantBuffer<UBO> step : register(b2, space0);

VSOutput mainVS(VSInput input)
{
    VSOutput output;
    
    output.Position = float4(input.Position, 0.0, 1.0);
    output.Color = input.Color;
    
    return output;
}

float4 mainPS(VSOutput input) : SV_TARGET
{
    return lerp(input.Color, lerp(begin.Value, end.Value, step.Value), 0.5f);
}";

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _beginBuffer = null!;
    private DeviceBuffer _endBuffer = null!;
    private DeviceBuffer _stepBuffer = null!;
    private ResourceLayout _resourceLayout = null!;
    private ResourceSet _resourceSet = null!;
    private Shader[] _shaders = null!;
    private VertexLayoutDescription[] _vertexLayoutDescriptions = null!;

    private Pipeline? _pipeline;

    protected override void Initialize()
    {
        Title = "Test Scene";

        Vertex[] triangleVertices =
        [
            new(new Vector2( 0.00f,  0.75f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
            new(new Vector2(-0.75f, -0.75f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
            new(new Vector2( 0.75f, -0.75f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f))
        ];

        ushort[] triangleIndices = [0, 1, 2];

        _vertexBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Vertex.SizeInBytes * triangleVertices.Length), BufferUsage.VertexBuffer));
        _indexBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * triangleIndices.Length), BufferUsage.IndexBuffer));
        _beginBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer));
        _endBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer));
        _stepBuffer = App.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        App.GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, [.. triangleVertices]);
        App.GraphicsDevice.UpdateBuffer(_indexBuffer, 0, [.. triangleIndices]);
        App.GraphicsDevice.UpdateBuffer(_beginBuffer, 0, [new Ubo { Value = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) }]);
        App.GraphicsDevice.UpdateBuffer(_endBuffer, 0, [new Ubo { Value = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) }]);
        App.GraphicsDevice.UpdateBuffer(_stepBuffer, 0, [new Ubo { Value = new Vector4(0.2f) }]);

        ResourceLayoutDescription resourceLayoutDescription = new(new ResourceLayoutElementDescription("Begin", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("End", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("Step", ResourceKind.UniformBuffer, ShaderStages.Fragment));

        _resourceLayout = App.ResourceFactory.CreateResourceLayout(resourceLayoutDescription);

        ResourceSetDescription resourceSetDescription = new(_resourceLayout, _beginBuffer, _endBuffer, _stepBuffer);

        _resourceSet = App.ResourceFactory.CreateResourceSet(resourceSetDescription);

        _shaders = App.ResourceFactory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(HLSL), "mainVS"),
                                                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(HLSL), "mainPS"));

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float4);

        _vertexLayoutDescriptions = [new VertexLayoutDescription(positionDescription, colorDescription)];
    }

    protected override void RecreatePipeline(Framebuffer framebuffer)
    {
        _pipeline?.Dispose();

        GraphicsPipelineDescription graphicsPipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout],
            ShaderSet = new ShaderSetDescription(_vertexLayoutDescriptions, _shaders),
            Outputs = framebuffer.OutputDescription
        };

        _pipeline = App.ResourceFactory.CreateGraphicsPipeline(graphicsPipelineDescription);
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
        App.GraphicsDevice.UpdateBuffer(_stepBuffer, 0, [new Ubo { Value = new Vector4(((float)Math.Sin(e.TotalTime) * 0.5f) + 0.5f) }]);
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
        if (_pipeline == null)
        {
            return;
        }

        commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        commandList.ClearDepthStencil(1.0f);

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U16);

        commandList.SetPipeline(_pipeline);

        commandList.SetGraphicsResourceSet(0, _resourceSet);

        commandList.DrawIndexed(indexCount: 3,
                                instanceCount: 1,
                                indexStart: 0,
                                vertexOffset: 0,
                                instanceStart: 0);
    }

    protected override void Destroy()
    {
        _pipeline?.Dispose();

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _resourceSet.Dispose();
        _resourceLayout.Dispose();
        _stepBuffer.Dispose();
        _endBuffer.Dispose();
        _beginBuffer.Dispose();
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();

        base.Destroy();
    }
}
