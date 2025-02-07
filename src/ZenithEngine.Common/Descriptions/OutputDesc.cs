using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct OutputDesc
{
    /// <summary>
    /// Color attachment formats.
    /// </summary>
    public PixelFormat[] ColorAttachments;

    /// <summary>
    /// Depth stencil attachment format.
    /// </summary>
    public PixelFormat? DepthStencilAttachment;

    /// <summary>
    /// The number of samples in each target attachment.
    /// </summary>
    public TextureSampleCount SampleCount;

    public static OutputDesc New(TextureSampleCount sampleCount = TextureSampleCount.Count1,
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
