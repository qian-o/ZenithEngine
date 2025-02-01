using Silk.NET.Core.Native;
using Silk.NET.DXGI;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.DirectX12;

internal class DXSwapChain : SwapChain
{
    public const uint FrameCount = 4;

    public ComPtr<IDXGISwapChain3> SwapChain3;

    public DXSwapChain(GraphicsContext context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
    }

    public override FrameBuffer FrameBuffer { get; }

    public override void Present()
    {
    }

    public override void Resize()
    {
    }

    public override void RefreshSurface(ISurface surface)
    {
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
    }
}
