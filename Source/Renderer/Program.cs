using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Hexa.NET.ImGui;

namespace Renderer;

internal sealed unsafe class Program
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

    private static Context _context = null!;
    private static GraphicsDevice _graphicsDevice = null!;
    private static ImGuiController _imGuiController = null!;
    private static DeviceBuffer _vertexBuffer = null!;
    private static DeviceBuffer _indexBuffer = null!;
    private static DeviceBuffer _beginBuffer = null!;
    private static DeviceBuffer _endBuffer = null!;
    private static DeviceBuffer _stepBuffer = null!;
    private static ResourceLayout _resourceLayout = null!;
    private static ResourceSet _resourceSet = null!;
    private static Shader[] _shaders = null!;
    private static Pipeline _pipeline = null!;
    private static CommandList _commandList = null!;

    private static void Main(string[] _)
    {
        using Window window = new();

        window.Load += Window_Load;
        window.Update += Window_Update;
        window.Render += Window_Render;
        window.Resize += Window_Resize;
        window.Close += Window_Close;

        window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        Window window = (Window)sender!;

        _context = new();

        foreach (PhysicalDevice physicalDevice in _context.EnumeratePhysicalDevices())
        {
            Console.WriteLine(physicalDevice.Name);

            _graphicsDevice = _context.CreateGraphicsDevice(physicalDevice, window);

            break;
        }

        _imGuiController = new(_graphicsDevice, window.IWindow, window.InputContext, new ImGuiFontConfig("Resources/Fonts/MSYH.TTC", 14, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()));

        ResourceFactory factory = _graphicsDevice.ResourceFactory;

        Vertex[] triangleVertices =
        [
            new(new Vector2( 0.00f,  0.75f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
            new(new Vector2(-0.75f, -0.75f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
            new(new Vector2( 0.75f, -0.75f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f))
        ];

        ushort[] triangleIndices = [0, 1, 2];

        _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(Vertex.SizeInBytes * triangleVertices.Length), BufferUsage.VertexBuffer));
        _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * triangleIndices.Length), BufferUsage.IndexBuffer));
        _beginBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer));
        _endBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer));
        _stepBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _graphicsDevice.UpdateBuffer(_vertexBuffer, 0, triangleVertices);
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, triangleIndices);
        _graphicsDevice.UpdateBuffer(_beginBuffer, 0, [new Ubo { Value = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) }]);
        _graphicsDevice.UpdateBuffer(_endBuffer, 0, [new Ubo { Value = new Vector4(0.0f, 0.0f, 1.0f, 1.0f) }]);
        _graphicsDevice.UpdateBuffer(_stepBuffer, 0, [new Ubo { Value = new Vector4(0.2f) }]);

        ResourceLayoutDescription resourceLayoutDescription = new(new ResourceLayoutElementDescription("Begin", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("End", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("Step", ResourceKind.UniformBuffer, ShaderStages.Fragment));

        _resourceLayout = factory.CreateResourceLayout(resourceLayoutDescription);

        ResourceSetDescription resourceSetDescription = new(_resourceLayout, _beginBuffer, _endBuffer, _stepBuffer);

        _resourceSet = factory.CreateResourceSet(resourceSetDescription);

        _shaders = factory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                                           new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main"));

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float4);

        VertexLayoutDescription vertexLayoutDescription = new(positionDescription, colorDescription);

        GraphicsPipelineDescription graphicsPipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout],
            ShaderSet = new ShaderSetDescription([vertexLayoutDescription], _shaders),
            Outputs = _graphicsDevice.Swapchain.OutputDescription
        };

        _pipeline = factory.CreateGraphicsPipeline(graphicsPipelineDescription);
        _commandList = factory.CreateGraphicsCommandList();
    }

    private static void Window_Update(object? sender, UpdateEventArgs e)
    {
        _graphicsDevice.UpdateBuffer(_stepBuffer, 0, [new Ubo { Value = new Vector4(((float)Math.Sin(e.TotalTime) * 0.5f) + 0.5f) }]);
    }

    private static void Window_Render(object? sender, RenderEventArgs e)
    {
        _imGuiController.Update(e.DeltaTime);

        ImGui.ShowDemoWindow();

        _commandList.Begin();

        _commandList.SetFramebuffer(_graphicsDevice.Swapchain.Framebuffer);

        _commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U16);

        _commandList.SetPipeline(_pipeline);

        _commandList.SetGraphicsResourceSet(0, _resourceSet);

        _commandList.DrawIndexed(indexCount: 3,
                                 instanceCount: 1,
                                 indexStart: 0,
                                 vertexOffset: 0,
                                 instanceStart: 0);

        _imGuiController.Render(_commandList);

        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);

        _graphicsDevice.SwapBuffers();
    }

    private static void Window_Resize(object? sender, ResizeEventArgs e)
    {
        _graphicsDevice.Resize(e.Width, e.Height);
    }

    private static void Window_Close(object? sender, CloseEventArgs e)
    {
        _imGuiController.Dispose();
        _commandList.Dispose();
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
        _graphicsDevice.Dispose();
        _context.Dispose();
    }
}