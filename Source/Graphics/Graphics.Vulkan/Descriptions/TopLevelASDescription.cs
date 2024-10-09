using Graphics.Core.RayTracing;
using Graphics.Vulkan.RayTracing;

namespace Graphics.Vulkan.Descriptions;

public record struct TopLevelASDescription
{
    public TopLevelASDescription(BuildMask mask, Instance[] instances, uint offset)
    {
        Mask = mask;
        Instances = instances;
        Offset = offset;
    }

    public TopLevelASDescription(BuildMask mask, params Instance[] instances) : this(mask, instances, 0)
    {
    }

    public BuildMask Mask { get; set; }

    public Instance[] Instances { get; set; }

    public uint Offset { get; set; }
}
