using Silk.NET.Core.Native;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.DirectX12;

internal unsafe class DXSwapChain : SwapChain
{
    public const uint BufferCount = 4;

    public ComPtr<IDXGISwapChain3> SwapChain3;
    public uint BackBufferIndex;

    private readonly DXSwapChainFrameBuffer swapChainFrameBuffer;

    public DXSwapChain(GraphicsContext context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
        swapChainFrameBuffer = new(Context, this);

        CreateSwapChain();
    }

    public override FrameBuffer FrameBuffer => swapChainFrameBuffer[BackBufferIndex];

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public override void Present()
    {
        SwapChain3.Present(Desc.VerticalSync ? 1u : 0u, DXGI.PresentAllowTearing).ThrowIfError();

        Context.DefaultGraphicsCommandProcessor!.WaitIdle();

        BackBufferIndex = SwapChain3.GetCurrentBackBufferIndex();
    }

    public override void Resize()
    {
        Context.DefaultGraphicsCommandProcessor!.WaitIdle();

        swapChainFrameBuffer.DestroyFrameBuffers();

        Vector2D<uint> size = Desc.Surface.GetSize();

        SwapChain3.ResizeBuffers(BufferCount,
                                 size.X,
                                 size.Y,
                                 DXFormats.GetFormat(Desc.ColorTargetFormat),
                                 (uint)SwapChainFlag.AllowTearing).ThrowIfError();

        swapChainFrameBuffer.CreateFrameBuffers(size.X, size.Y);

        BackBufferIndex = SwapChain3.GetCurrentBackBufferIndex();
    }

    public override void RefreshSurface(ISurface surface)
    {
        Desc.Surface = surface;

        CreateSwapChain();
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        swapChainFrameBuffer.Dispose();

        DestroySwapChain();
    }

    private void CreateSwapChain()
    {
        DestroySwapChain();

        Vector2D<uint> size = Desc.Surface.GetSize();

        SwapChainDesc1 desc = new()
        {
            Width = size.X,
            Height = size.Y,
            Format = DXFormats.GetFormat(Desc.ColorTargetFormat),
            SampleDesc = new(1, 0),
            BufferUsage = DXGI.UsageRenderTargetOutput,
            BufferCount = BufferCount,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipDiscard,
            AlphaMode = AlphaMode.Ignore,
            Flags = (uint)SwapChainFlag.AllowTearing
        };

        Context.Factory6.CreateSwapChainForHwnd(Context.DefaultGraphicsCommandProcessor!.Queue,
                                                Desc.Surface.Handles[0],
                                                &desc,
                                                null,
                                                (ComPtr<IDXGIOutput>)null,
                                                ref SwapChain3).ThrowIfError();

        swapChainFrameBuffer.CreateFrameBuffers(size.X, size.Y);

        BackBufferIndex = SwapChain3.GetCurrentBackBufferIndex();
    }

    private void DestroySwapChain()
    {
        if (SwapChain3.Handle is null)
        {
            return;
        }

        swapChainFrameBuffer.DestroyFrameBuffers();

        SwapChain3.Dispose();
    }
}
