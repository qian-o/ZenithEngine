using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct ComputePipelineDesc(Shader shader,
                                  params ResourceLayout[] resourceLayouts)
{
    public ComputePipelineDesc()
    {
    }

    /// <summary>
    /// The compute shader to be used.
    /// </summary>
    public Shader Shader = shader;

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts = resourceLayouts;
}
