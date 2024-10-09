using System.Numerics;
using Graphics.Core;

namespace Graphics.Vulkan.RayTracing;

public class Triangles : Geometry
{
    public required DeviceBuffer VertexBuffer { get; set; }

    public PixelFormat VertexFormat { get; set; }

    public uint VertexStride { get; set; }

    public uint VertexCount { get; set; }

    public uint VertexOffset { get; set; }

    public DeviceBuffer? IndexBuffer { get; set; }

    public IndexFormat IndexFormat { get; set; }

    public uint IndexCount { get; set; }

    public uint IndexOffset { get; set; }

    public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
}
