using Silk.NET.DXGI;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXSwapChainFrameBuffer(GraphicsContext context,
                                      DXSwapChain swapChain) : GraphicsResource(context)
{
    private Texture? depthStencilTarget;
    private Texture[] colorTargets = [];
    private FrameBuffer[] frameBuffers = [];

    public FrameBuffer this[uint index] => frameBuffers[index];

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void CreateFrameBuffers(uint width, uint height, Format imageFormat)
    {
    }

    public void DestroyFrameBuffers()
    {
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        DestroyFrameBuffers();
    }
}
