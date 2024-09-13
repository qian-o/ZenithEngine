using Graphics.Core;
using Graphics.Vulkan;

namespace Tests.Core;

public class FramebufferObject : DisposableObject
{
    private const TextureSampleCount MaxSampleCount = TextureSampleCount.Count8;
    private readonly Texture _colorTexture;
    private readonly Texture _depthTexture;

    public FramebufferObject(GraphicsDevice device, int width, int height)
    {
        ResourceFactory factory = device.Factory;

        Width = width;
        Height = height;
        _colorTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                                                                           (uint)height,
                                                                           1,
                                                                           PixelFormat.R8G8B8A8UNorm,
                                                                           TextureUsage.RenderTarget,
                                                                           MaxSampleCount));

        _depthTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                                                                           (uint)height,
                                                                           1,
                                                                           PixelFormat.D32FloatS8UInt,
                                                                           TextureUsage.DepthStencil,
                                                                           MaxSampleCount));

        PresentTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                                                                             (uint)height,
                                                                             1,
                                                                             PixelFormat.R8G8B8A8UNorm,
                                                                             TextureUsage.Sampled,
                                                                             TextureSampleCount.Count1));

        Framebuffer = factory.CreateFramebuffer(new FramebufferDescription(_depthTexture, _colorTexture));
    }

    public int Width { get; }

    public int Height { get; }

    public Framebuffer Framebuffer { get; }

    public Texture PresentTexture { get; }

    public void Present(CommandList commandList)
    {
        commandList.ResolveTexture(_colorTexture, PresentTexture);
    }

    protected override void Destroy()
    {
        Framebuffer.Dispose();
        PresentTexture.Dispose();
        _depthTexture.Dispose();
        _colorTexture.Dispose();
    }
}
