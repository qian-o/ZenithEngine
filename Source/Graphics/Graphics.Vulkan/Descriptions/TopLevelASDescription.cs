using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct TopLevelASDescription
{
    public TopLevelASDescription(AccelerationStructureOptions options, AccelerationStructureInstance[] instances, uint offset)
    {
        Options = options;
        Instances = instances;
        Offset = offset;
    }

    public TopLevelASDescription(AccelerationStructureOptions options, params AccelerationStructureInstance[] instances) : this(options, instances, 0)
    {
    }

    public AccelerationStructureOptions Options { get; set; }

    public AccelerationStructureInstance[] Instances { get; set; }

    public uint Offset { get; set; }
}
