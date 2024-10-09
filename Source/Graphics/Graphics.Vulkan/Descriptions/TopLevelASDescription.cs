using Graphics.Core.RayTracing;
using Graphics.Vulkan.RayTracing;

namespace Graphics.Vulkan.Descriptions;

public record struct TopLevelASDescription
{
    public TopLevelASDescription(AccelStructBuildMask mask, AccelStructInstance[] instances, uint offset)
    {
        Mask = mask;
        Instances = instances;
        Offset = offset;
    }

    public TopLevelASDescription(AccelStructBuildMask mask, params AccelStructInstance[] instances) : this(mask, instances, 0)
    {
    }

    public AccelStructBuildMask Mask { get; set; }

    public AccelStructInstance[] Instances { get; set; }

    public uint Offset { get; set; }
}
