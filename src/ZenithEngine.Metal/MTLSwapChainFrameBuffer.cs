using Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLSwapChainFrameBuffer : GraphicsResource
{
    private readonly MTLSwapChain swapChain;
    private MTLFrameBuffer? frameBuffer;
    private MTLTexture? colorTexture;
    private MTLTexture? depthStencilTexture;
    private uint width;
    private uint height;

    public MTLSwapChainFrameBuffer(GraphicsContext context, MTLSwapChain swapChain) : base(context)
    {
        this.swapChain = swapChain;
        
        var size = swapChain.Desc.Surface.GetSize();
        UpdateSize(size.X, size.Y);
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public FrameBuffer FrameBuffer => frameBuffer 
        ?? throw new InvalidOperationException("FrameBuffer is not initialized");

    public void UpdateSize(uint newWidth, uint newHeight)
    {
        if (width == newWidth && height == newHeight && frameBuffer is not null)
        {
            return;
        }

        width = newWidth;
        height = newHeight;

        // Dispose old resources
        frameBuffer?.Dispose();
        depthStencilTexture?.Dispose();

        // Create depth/stencil texture if needed
        if (swapChain.Desc.DepthStencilTargetFormat.HasValue)
        {
            TextureDesc depthDesc = new(
                TextureType.Texture2D,
                width,
                height,
                1,
                1,
                1,
                swapChain.Desc.DepthStencilTargetFormat.Value,
                TextureUsage.DepthStencil,
                TextureSampleCount.Count1);

            depthStencilTexture = new MTLTexture(Context, in depthDesc);
        }

        // Create a wrapper framebuffer
        // Note: The actual color attachment will be updated each frame with the current drawable
        CreateFrameBuffer();
    }

    /// <summary>
    /// Updates the framebuffer with the current drawable texture.
    /// </summary>
    public void UpdateDrawable(ICAMetalDrawable drawable)
    {
        // Wrap the drawable's texture in our MTLTexture wrapper
        IMTLTexture drawableTexture = drawable.Texture;
        
        // Create a temporary TextureDesc for the drawable
        TextureDesc colorDesc = new(
            TextureType.Texture2D,
            (uint)drawableTexture.Width,
            (uint)drawableTexture.Height,
            1,
            1,
            1,
            swapChain.Desc.ColorTargetFormat,
            TextureUsage.RenderTarget,
            TextureSampleCount.Count1);

        // Create wrapper for drawable texture
        // We need a special constructor for this
        colorTexture?.Dispose();
        colorTexture = new MTLTexture(Context, in colorDesc);
        
        // TODO: Assign the drawable texture to our wrapper
        // This requires extending MTLTexture to support external textures

        CreateFrameBuffer();
    }

    protected override void SetName(string name)
    {
        frameBuffer?.SetLabel($"{name}_FrameBuffer");
    }

    protected override void Destroy()
    {
        frameBuffer?.Dispose();
        colorTexture?.Dispose();
        depthStencilTexture?.Dispose();
    }

    private void CreateFrameBuffer()
    {
        frameBuffer?.Dispose();

        List<FrameBufferAttachmentDesc> colorTargets = [];
        
        // Color attachment will be updated with drawable each frame
        // For now, create a placeholder framebuffer structure
        
        FrameBufferAttachmentDesc? depthTarget = null;
        if (depthStencilTexture is not null)
        {
            depthTarget = new FrameBufferAttachmentDesc(depthStencilTexture);
        }

        // Create framebuffer descriptor
        // Note: This is a simplified version - actual implementation would need
        // to handle the drawable texture properly
        FrameBufferDesc fbDesc = new(depthTarget);
        
        frameBuffer = new MTLFrameBuffer(Context, in fbDesc);
    }
}
