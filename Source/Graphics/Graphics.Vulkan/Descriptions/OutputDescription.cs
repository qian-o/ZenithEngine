using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct OutputDescription
{
    public OutputDescription()
    {
    }

    /// <summary>
    /// A description of the depth attachment, or null if none exists.
    /// </summary>
    public OutputAttachmentDescription? DepthAttachment { get; }

    /// <summary>
    /// An array of attachment descriptions, one for each color attachment. May be empty.
    /// </summary>
    public OutputAttachmentDescription[] ColorAttachments { get; } = [];

    /// <summary>
    /// The number of samples in each target attachment.
    /// </summary>
    public TextureSampleCount SampleCount { get; }
}
