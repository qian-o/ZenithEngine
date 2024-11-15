namespace ZenithEngine.Common.Graphics;

public class AccelerationStructureAABBs(Buffer aabbs) : AccelerationStructureGeometry
{
    public Buffer AABBs { get; } = aabbs;

    public ulong Count { get; set; }

    public uint Stride { get; set; }

    public uint Offset { get; set; }
}
