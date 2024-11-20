using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class FrameBuffer(GraphicsContext context,
                                  ref readonly FrameBufferDesc desc) : GraphicsResource(context)
{
    public FrameBufferDesc Desc { get; } = desc;

    public abstract uint Width { get; }

    public abstract uint Height { get; }

    public abstract OutputDesc Output { get; }
}
