namespace Graphics.Engine.Descriptions;

public struct FrameBufferDescription(FrameBufferAttachmentDescription? depthStencilTarget,
                                     params FrameBufferAttachmentDescription[] colorTargets)
{
    /// <summary>
    /// The depth/stencil texture attachment.
    /// </summary>
    public FrameBufferAttachmentDescription? DepthStencilTarget { get; set; } = depthStencilTarget;

    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FrameBufferAttachmentDescription[] ColorTargets { get; set; } = colorTargets;

    public static FrameBufferDescription Create(Texture? depthStencilTarget,
                                                params Texture[] colorTargets)
    {
        return new FrameBufferDescription(depthStencilTarget != null ? FrameBufferAttachmentDescription.Create(depthStencilTarget) : null,
                                          [.. colorTargets.Select(FrameBufferAttachmentDescription.Create)]);
    }
}
