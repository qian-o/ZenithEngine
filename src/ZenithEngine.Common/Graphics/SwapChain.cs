using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Common.Graphics;

public abstract class SwapChain(GraphicsContext context,
                                ref readonly SwapChainDesc desc) : GraphicsResource(context)
{
    private SwapChainDesc descInternal = desc;

    public ref SwapChainDesc Desc => ref descInternal;

    public abstract FrameBuffer FrameBuffer { get; }

    public abstract void Present();

    public abstract void Resize();

    public abstract void RefreshSurface(ISurface surface);
}
