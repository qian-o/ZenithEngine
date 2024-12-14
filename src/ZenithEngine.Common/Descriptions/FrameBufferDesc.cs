namespace ZenithEngine.Common.Descriptions;

public struct FrameBufferDesc
{
    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FrameBufferAttachmentDesc[] ColorTargets;

    /// <summary>
    /// The depth/stencil texture attachment.
    /// </summary>
    public FrameBufferAttachmentDesc? DepthStencilTarget;

    public static FrameBufferDesc Default(FrameBufferAttachmentDesc? depthStencilTarget,
                                          params FrameBufferAttachmentDesc[] colorTargets)
    {
        return new()
        {
            ColorTargets = colorTargets,
            DepthStencilTarget = depthStencilTarget
        };
    }
}
