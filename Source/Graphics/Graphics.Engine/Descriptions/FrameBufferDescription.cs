namespace Graphics.Engine.Descriptions;

public struct FrameBufferDescription(FrameBufferAttachmentDescription? depthTarget,
                                     params FrameBufferAttachmentDescription[] colorTargets)
{
    /// <summary>
    /// The depth texture attachment.
    /// </summary>
    public FrameBufferAttachmentDescription? DepthTarget { get; set; } = depthTarget;

    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FrameBufferAttachmentDescription[] ColorTargets { get; set; } = colorTargets;

    public static FrameBufferDescription Create(Texture? depthTarget,
                                                params Texture[] colorTargets)
    {
        return new FrameBufferDescription(depthTarget != null ? FrameBufferAttachmentDescription.Create(depthTarget) : null,
                                          [.. colorTargets.Select(FrameBufferAttachmentDescription.Create)]);
    }
}
