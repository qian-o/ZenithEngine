using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct OutputDesc(TextureSampleCount sampleCount = TextureSampleCount.Count1,
                         PixelFormat? depthStencilAttachment = null,
                         params PixelFormat[] colorAttachments)
{
    /// <summary>
    /// Color attachment formats.
    /// </summary>
    public PixelFormat[] ColorAttachments = colorAttachments;

    /// <summary>
    /// Depth stencil attachment format.
    /// </summary>
    public PixelFormat? DepthStencilAttachment = depthStencilAttachment;

    /// <summary>
    /// The number of samples in each target attachment.
    /// </summary>
    public TextureSampleCount SampleCount = sampleCount;
}
