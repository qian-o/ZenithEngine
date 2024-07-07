using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;

internal sealed class Program
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
    fsout_Color = mix(begin.Color, end.Color, step.Value);
}";

    private static Context _context = null!;
    private static GraphicsDevice _graphicsDevice = null!;
    private static DeviceBuffer _vertexBuffer = null!;
    private static DeviceBuffer _indexBuffer = null!;
    private static DeviceBuffer _beginBuffer = null!;
    private static DeviceBuffer _endBuffer = null!;
    private static DeviceBuffer _stepBuffer = null!;
    private static ResourceLayout _resourceLayout = null!;
    private static ResourceSet _resourceSet = null!;
    private static Shader[] _shaders = null!;

    private static void Main(string[] _)
    {
        using Window window = new();

        window.Load += Window_Load;
        window.Resize += Window_Resize;
        window.Close += Window_Close;

        window.Run();
    }

    private static void Window_Load(object? sender, LoadEventArgs e)
    {
        _context = new();

        foreach (PhysicalDevice physicalDevice in _context.EnumeratePhysicalDevices())
        {
            Console.WriteLine(physicalDevice.Name);

            _graphicsDevice = _context.CreateGraphicsDevice(physicalDevice, (Window)sender!);

            break;
        }

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
        _stepBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Ubo>(), BufferUsage.UniformBuffer));

        ResourceLayoutDescription resourceLayoutDescription = new(new ResourceLayoutElementDescription("Begin", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("End", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                                                                  new ResourceLayoutElementDescription("Step", ResourceKind.UniformBuffer, ShaderStages.Fragment));

        _resourceLayout = factory.CreateResourceLayout(resourceLayoutDescription);

        ResourceSetDescription resourceSetDescription = new(_resourceLayout, _beginBuffer, _endBuffer, _stepBuffer);

        _resourceSet = factory.CreateResourceSet(resourceSetDescription);

        _shaders = factory.CreateFromSpirv(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                                           new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main"));
    }

    private static void Window_Resize(object? sender, ResizeEventArgs e)
    {
        _graphicsDevice.Resize(e.Width, e.Height);
    }

    private static void Window_Close(object? sender, CloseEventArgs e)
    {
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