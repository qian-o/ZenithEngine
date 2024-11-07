namespace Graphics.Engine.Descriptions;

public struct FrameBufferDescription
{
    /// <summary>
    /// The depth/stencil texture attachment.
    /// </summary>
    public FrameBufferAttachmentDescription? DepthStencilTarget { get; set; }

    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FrameBufferAttachmentDescription[] ColorTargets { get; set; }

    public static FrameBufferDescription Default(Texture? depthStencilTarget,
                                                 params Texture[] colorTargets)
    {
        return new()
        {
            DepthStencilTarget = depthStencilTarget != null ? FrameBufferAttachmentDescription.Default(depthStencilTarget) : null,
            ColorTargets = colorTargets.Select(FrameBufferAttachmentDescription.Default).ToArray()
        };
    }
}
