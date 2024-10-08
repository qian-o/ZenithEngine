using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct TopLevelASDescription
{
    public TopLevelASDescription(AsBuildMask mask, AccelerationStructureInstance[] instances, uint offset)
    {
        Mask = mask;
        Instances = instances;
        Offset = offset;
    }

    public TopLevelASDescription(AsBuildMask mask, params AccelerationStructureInstance[] instances) : this(mask, instances, 0)
    {
    }

    public AsBuildMask Mask { get; set; }

    public AccelerationStructureInstance[] Instances { get; set; }

    public uint Offset { get; set; }
}
