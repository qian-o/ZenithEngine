using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Common.Graphics;

public abstract class SwapChain(GraphicsContext context,
                                ref readonly SwapChainDesc desc) : GraphicsResource(context)
{
    private SwapChainDesc descInternal = desc;

    public ref SwapChainDesc Desc => ref descInternal;

    public abstract FrameBuffer FrameBuffer { get; }

    /// <summary>
    /// Present the swap chain.
    /// </summary>
    public abstract void Present();

    /// <summary>
    /// Resize the swap chain.
    /// </summary>
    public abstract void Resize();

    /// <summary>
    /// Refresh the swap chain surface.
    /// </summary>
    /// <param name="surface">New surface.</param>
    public abstract void RefreshSurface(ISurface surface);
}
