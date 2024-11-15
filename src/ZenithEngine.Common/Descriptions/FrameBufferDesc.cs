using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Common.Descriptions;

public struct FrameBufferDesc
{
    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FrameBufferAttachmentDesc[] ColorTargets { get; set; }

    /// <summary>
    /// The depth/stencil texture attachment.
    /// </summary>
    public FrameBufferAttachmentDesc? DepthStencilTarget { get; set; }

    public static FrameBufferDesc Default(Texture? depthStencilTarget,
                                          params Texture[] colorTargets)
    {
        return new()
        {
            ColorTargets = colorTargets.Select(item => FrameBufferAttachmentDesc.Default(item)).ToArray(),
            DepthStencilTarget = depthStencilTarget is not null ? FrameBufferAttachmentDesc.Default(depthStencilTarget) : null
        };
    }
}
