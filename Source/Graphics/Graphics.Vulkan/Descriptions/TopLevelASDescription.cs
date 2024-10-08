using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct TopLevelASDescription
{
    public TopLevelASDescription(ASBuildMask mask, ASInstance[] instances, uint offset)
    {
        Mask = mask;
        Instances = instances;
        Offset = offset;
    }

    public TopLevelASDescription(ASBuildMask mask, params ASInstance[] instances) : this(mask, instances, 0)
    {
    }

    public ASBuildMask Mask { get; set; }

    public ASInstance[] Instances { get; set; }

    public uint Offset { get; set; }
}
