namespace ZenithEngine.Common.Graphics;

public class AccelerationStructureAABBs(Buffer aabbs) : AccelerationStructureGeometry
{
    public Buffer AABBs { get; } = aabbs;

    public uint Count { get; set; }

    public uint StrideInBytes { get; set; }

    public uint OffsetInBytes { get; set; }
}
