using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct ComputePipelineDesc
{
    /// <summary>
    /// The compute shader to be used.
    /// </summary>
    public Shader Shader { get; set; }

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; set; }

    public static ComputePipelineDesc Default(Shader shader,
                                              params ResourceLayout[] resourceLayouts)
    {
        return new()
        {
            Shader = shader,
            ResourceLayouts = resourceLayouts
        };
    }
}
