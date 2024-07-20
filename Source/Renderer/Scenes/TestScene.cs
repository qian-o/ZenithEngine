using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Renderer.Components;

namespace Renderer.Scenes;

internal sealed class TestScene(MainWindow mainWindow) : Scene(mainWindow)
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

    private const string VertexCode = @"
#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

    private const string FragmentCode = @"
#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

layout(std140, binding = 0) uniform Begin
{
    vec4 Color;
}begin;

layout(std140, binding = 1) uniform End
{
    vec4 Color;
}end;

layout(std140, binding = 2) uniform Step
{
    vec4 Value;
}step;

void main()
{
    fsout_Color = mix(fsin_Color, mix(begin.Color, end.Color, step.Value), 0.5);
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
    private Pipeline _pipeline = null!;

    protected override void Initialize()
    {
        Vertex[] triangleVertices =
        [
            new(new Vector2( 0.00f,  0.75f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
            new(new Vector2(-0.75f, -0.75f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
            new(new Vector2( 0.75f, -0.75f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f))
        ];

        ushort[] triangleIndices = [0, 1, 2];

        _vertexBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)(Vertex.SizeInBytes * triangleVertices.Length), BufferUsage.VertexBuffer));
        _indexBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * triangleIndices.Length), BufferUsage.IndexBuffer));
        _beginBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer));
        _endBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer));
        _stepBuffer = _resourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, triangleVertices);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, triangleIndices);
        _graphicsDevice.UpdateBuffer(_beginBuffer, 0, [new Ubo { Value = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) }]);
        _graphicsDevice.UpdateBuffer(_endBuffer, 0, [new Ubo { Value = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) }]);
        _graphicsDevice.UpdateBuffer(_stepBuffer, 0, [new Ubo { Value = new Vector4(0.2f) }]);

        ResourceLayoutDescription resourceLayoutDescription = new(new ResourceLayoutElementDescription("Begin", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("End", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("Step", ResourceKind.UniformBuffer, ShaderStages.Fragment));

        _resourceLayout = _resourceFactory.CreateResourceLayout(resourceLayoutDescription);

        ResourceSetDescription resourceSetDescription = new(_resourceLayout, _beginBuffer, _endBuffer, _stepBuffer);

        _resourceSet = _resourceFactory.CreateResourceSet(resourceSetDescription);

        _shaders = _resourceFactory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                                                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main"));

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float4);

        _vertexLayoutDescriptions = [new VertexLayoutDescription(positionDescription, colorDescription)];
    }

    protected override void UpdatePipeline(Framebuffer framebuffer)
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

        _pipeline = _resourceFactory.CreateGraphicsPipeline(graphicsPipelineDescription);
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
        _graphicsDevice.UpdateBuffer(_stepBuffer, 0, [new Ubo { Value = new Vector4(((float)Math.Sin(e.TotalTime) * 0.5f) + 0.5f) }]);
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
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
        _pipeline.Dispose();
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
