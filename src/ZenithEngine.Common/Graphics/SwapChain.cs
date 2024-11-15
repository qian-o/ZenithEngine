using ZenithEngine.Common.Descriptions;

namespace ZenithEngine.Common.Graphics;

public abstract class SwapChain(GraphicsContext context,
                                ref readonly SwapChainDesc desc) : GraphicsResource(context)
{
    public SwapChainDesc Desc { get; } = desc;

    public abstract FrameBuffer FrameBuffer { get; }

    public abstract void Present();

    public abstract void Resize();
}
