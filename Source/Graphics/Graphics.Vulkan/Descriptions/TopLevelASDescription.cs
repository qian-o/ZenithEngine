using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct TopLevelASDescription
{
    public TopLevelASDescription(AccelerationStructureBuildMask mask, AccelerationStructureInstance[] instances, uint offset)
    {
        Mask = mask;
        Instances = instances;
        Offset = offset;
    }

    public TopLevelASDescription(AccelerationStructureBuildMask mask, params AccelerationStructureInstance[] instances) : this(mask, instances, 0)
    {
    }

    public AccelerationStructureBuildMask Mask { get; set; }

    public AccelerationStructureInstance[] Instances { get; set; }

    public uint Offset { get; set; }
}
