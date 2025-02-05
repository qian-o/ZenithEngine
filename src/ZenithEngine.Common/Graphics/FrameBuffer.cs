using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class FrameBuffer(GraphicsContext context,
                                  ref readonly FrameBufferDesc desc) : GraphicsResource(context)
{
    private FrameBufferDesc descInternal = desc;

    public ref FrameBufferDesc Desc => ref descInternal;

    public abstract uint ColorAttachmentCount { get; }

    public abstract bool HasDepthStencilAttachment { get; }

    public abstract uint Width { get; }

    public abstract uint Height { get; }

    public abstract OutputDesc Output { get; }
}
