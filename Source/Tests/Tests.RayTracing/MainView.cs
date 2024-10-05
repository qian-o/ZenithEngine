using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using Tests.Core;

namespace Tests.RayTracing;

internal sealed unsafe class MainView : View
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex(Vector3 position, Vector3 normal, Vector3 color)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector3 Color = color;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CameraProperties
    {
        public Matrix4x4 ViewInverse;

        public Matrix4x4 ProjInverse;
    }

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;

    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _buffer;
    private readonly BottomLevelAS _bottomLevelAS;
    private readonly TopLevelAS _topLevelAS;
    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Ray Tracing")
    {
        Vertex[] vertices =
        [
            new(new Vector3(-1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 0.0f)),
            new(new Vector3(1.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f)),
            new(new Vector3(1.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f))
        ];
        ushort[] indices = [0, 1, 2];

        _device = device;
        _imGuiController = imGuiController;

        _vertexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Length, BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure));
        device.UpdateBuffer(_vertexBuffer, vertices);

        _indexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<ushort>(indices.Length, BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure));
        device.UpdateBuffer(_indexBuffer, indices);

        _buffer = device.Factory.CreateBuffer(BufferDescription.Buffer<CameraProperties>(1, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        AccelerationStructureTriangles accelerationStructureTriangles = new()
        {
            VertexBuffer = _vertexBuffer,
            VertexFormat = PixelFormat.R32G32B32Float,
            VertexStride = (uint)sizeof(Vertex),
            VertexCount = (uint)vertices.Length,
            VertexOffset = 0,
            IndexBuffer = _indexBuffer,
            IndexFormat = IndexFormat.U16,
            IndexCount = (uint)indices.Length,
            IndexOffset = 0,
        };

        _bottomLevelAS = device.Factory.CreateBottomLevelAS(new BottomLevelASDescription(accelerationStructureTriangles));

        AccelerationStructureInstance accelerationStructureInstance = new()
        {
            Transform4x4 = Matrix4x4.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelerationStructureInstanceOptions.None,
            BottonLevel = _bottomLevelAS
        };

        _topLevelAS = device.Factory.CreateTopLevelAS(new TopLevelASDescription(AccelerationStructureOptions.AllowUpdate, accelerationStructureInstance));

        _resourceLayout = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(new ResourceLayoutElementDescription("rs", ResourceKind.AccelerationStructure, ShaderStages.RayGeneration),
                                                                                            new ResourceLayoutElementDescription("outputTexture", ResourceKind.StorageImage, ShaderStages.RayGeneration),
                                                                                            new ResourceLayoutElementDescription("cameraProperties", ResourceKind.UniformBuffer, ShaderStages.RayGeneration)));
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
    }

    protected override void OnRender(RenderEventArgs e)
    {
    }

    protected override void OnResize(ResizeEventArgs e)
    {
    }

    protected override void Destroy()
    {
    }
}
