namespace Graphics.Vulkan;

public class AccelerationStructureAABBs : AccelerationStructureGeometry
{
    public required DeviceBuffer AABBs { get; set; }

    public ulong Count { get; set; }

    public uint Stride { get; set; }

    public uint Offset { get; set; }
}
