using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct OutputDesc
{
    /// <summary>
    /// Color attachment formats.
    /// </summary>
    public PixelFormat[] ColorAttachments { get; set; }

    /// <summary>
    /// Depth stencil attachment format.
    /// </summary>
    public PixelFormat? DepthStencilAttachment { get; set; }

    /// <summary>
    /// The number of samples in each target attachment.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; }

    public static OutputDesc Default(TextureSampleCount sampleCount = TextureSampleCount.Count1,
                                     PixelFormat? depthStencilAttachment = null,
                                     params PixelFormat[] colorAttachments)
    {
        return new()
        {
            ColorAttachments = colorAttachments,
            DepthStencilAttachment = depthStencilAttachment,
            SampleCount = sampleCount
        };
    }
}
