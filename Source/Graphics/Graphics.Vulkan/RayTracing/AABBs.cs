namespace Graphics.Vulkan.RayTracing;

public class AABBs : Geometry
{
    public required DeviceBuffer Buffer { get; set; }

    public ulong Count { get; set; }

    public uint Stride { get; set; }

    public uint Offset { get; set; }
}
