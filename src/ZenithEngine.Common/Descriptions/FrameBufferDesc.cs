namespace ZenithEngine.Common.Descriptions;

public struct FrameBufferDesc(FrameBufferAttachmentDesc? depthStencilTarget,
                              params FrameBufferAttachmentDesc[] colorTargets)
{
    public FrameBufferDesc() : this(null, [])
    {
    }

    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FrameBufferAttachmentDesc[] ColorTargets = colorTargets;

    /// <summary>
    /// The depth/stencil texture attachment.
    /// </summary>
    public FrameBufferAttachmentDesc? DepthStencilTarget = depthStencilTarget;
}
