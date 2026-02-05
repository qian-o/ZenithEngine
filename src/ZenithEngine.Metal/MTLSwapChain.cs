using Metal;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Metal;

internal class MTLSwapChain : SwapChain
{
    private readonly MTLFence fence;
    private MTLSwapChainFrameBuffer? swapChainFrameBuffer;
    private CAMetalLayer? metalLayer;
    private ICAMetalDrawable? currentDrawable;

    public MTLSwapChain(GraphicsContext context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
        fence = new(Context);

        CreateSwapChain();

        swapChainFrameBuffer = new(Context, this);
    }

    public override FrameBuffer FrameBuffer => swapChainFrameBuffer 
        ?? throw new InvalidOperationException("SwapChain framebuffer is not initialized");

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public ICAMetalDrawable? CurrentDrawable => currentDrawable;

    public override void Present()
    {
        if (currentDrawable is not null)
        {
            // The drawable will be presented when the command buffer commits
            // Metal automatically handles presentation via the command buffer
            currentDrawable = null;
        }

        fence.Wait(Context.GraphicsQueue);
    }

    public override void Resize()
    {
        fence.Wait(Context.GraphicsQueue);

        Vector2D<uint> size = Desc.Surface.GetSize();

        if (metalLayer is not null)
        {
            metalLayer.DrawableSize = new(size.X, size.Y);
        }

        swapChainFrameBuffer?.UpdateSize(size.X, size.Y);
    }

    public override void RefreshSurface(ISurface surface)
    {
        fence.Wait(Context.GraphicsQueue);

        Desc.Surface = surface;

        CreateSwapChain();

        swapChainFrameBuffer?.UpdateSize(surface.GetSize().X, surface.GetSize().Y);
    }

    /// <summary>
    /// Gets the next drawable from the Metal layer.
    /// </summary>
    public ICAMetalDrawable? NextDrawable()
    {
        if (metalLayer is null)
        {
            return null;
        }

        currentDrawable = metalLayer.NextDrawable();
        return currentDrawable;
    }

    protected override void SetName(string name)
    {
        metalLayer?.SetLabel(name);
    }

    protected override void Destroy()
    {
        fence.Dispose();
        swapChainFrameBuffer?.Dispose();
        currentDrawable?.Dispose();
        metalLayer?.Dispose();
    }

    private void CreateSwapChain()
    {
        // Get the native window handle and create CAMetalLayer
        nint windowHandle = Desc.Surface.GetHandle();
        
        // Create or get CAMetalLayer for the window
        // Note: The actual CAMetalLayer creation depends on the platform (iOS/macOS)
        // and how the window system exposes the native layer
        metalLayer = CAMetalLayer.Create();
        
        if (metalLayer is not null)
        {
            metalLayer.Device = Context.Device;
            metalLayer.PixelFormat = MTLFormats.GetMTLPixelFormat(Desc.ColorTargetFormat);
            metalLayer.FramebufferOnly = true; // Optimize for rendering
            
            Vector2D<uint> size = Desc.Surface.GetSize();
            metalLayer.DrawableSize = new(size.X, size.Y);

            // Disable VSync if requested (use immediate presentation)
            if (!Desc.VerticalSync)
            {
                metalLayer.DisplaySyncEnabled = false;
            }
        }
    }
}
