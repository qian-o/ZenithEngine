using Graphics.Core;
using Graphics.Vulkan;

namespace Tests.Core;

public class FramebufferObject : DisposableObject
{
    private const TextureSampleCount MaxSampleCount = TextureSampleCount.Count8;

    private readonly int _width;
    private readonly int _height;
    private readonly Texture _colorTexture;
    private readonly Texture _depthTexture;
    private readonly Texture _presentTexture;
    private readonly Framebuffer _framebuffer;

    public FramebufferObject(GraphicsDevice device, int width, int height)
    {
        ResourceFactory factory = device.ResourceFactory;

        _width = width;
        _height = height;
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

        _presentTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                                                                             (uint)height,
                                                                             1,
                                                                             PixelFormat.R8G8B8A8UNorm,
                                                                             TextureUsage.Sampled,
                                                                             TextureSampleCount.Count1));

        _framebuffer = factory.CreateFramebuffer(new FramebufferDescription(_colorTexture, _depthTexture));
    }

    public int Width => _width;

    public int Height => _height;

    public Framebuffer Framebuffer => _framebuffer;

    public Texture PresentTexture => _presentTexture;

    public void Present(CommandList commandList)
    {
        commandList.ResolveTexture(_colorTexture, _presentTexture);
    }

    protected override void Destroy()
    {
        _framebuffer.Dispose();
        _presentTexture.Dispose();
        _depthTexture.Dispose();
        _colorTexture.Dispose();
    }
}
