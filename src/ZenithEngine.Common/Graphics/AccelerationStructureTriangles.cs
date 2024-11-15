using Silk.NET.Maths;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public class AccelerationStructureTriangles(Buffer vertexBuffer) : AccelerationStructureGeometry
{
    public Buffer VertexBuffer { get; } = vertexBuffer;

    public PixelFormat VertexFormat { get; set; }

    public uint VertexStrideInBytes { get; set; }

    public uint VertexCount { get; set; }

    public uint VertexOffsetInBytes { get; set; }

    public Buffer? IndexBuffer { get; set; }

    public IndexFormat IndexFormat { get; set; }

    public uint IndexCount { get; set; }

    public uint IndexOffsetInBytes { get; set; }

    public Matrix4X4<float> Transform { get; set; }
}
