using Graphics.Core;
using Graphics.Vulkan;

namespace Renderer.Components;

internal sealed class FBO : DisposableObject
{
    private readonly uint _width;
    private readonly uint _height;
    private readonly PixelFormat _colorFormat;
    private readonly PixelFormat _depthFormat;
    private readonly TextureSampleCount _sampleCount;
    private readonly Texture _colorTexture;
    private readonly Texture _depthTexture;
    private readonly Framebuffer _framebuffer;
    private readonly Texture _presentTexture;

    public FBO(ResourceFactory resourceFactory,
               uint width,
               uint height,
               PixelFormat colorFormat = PixelFormat.R8G8B8A8UNorm,
               PixelFormat depthFormat = PixelFormat.D32FloatS8UInt,
               TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        _width = width;
        _height = height;
        _colorFormat = colorFormat;
        _depthFormat = depthFormat;
        _sampleCount = sampleCount;
        _colorTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(width,
                                                                                   height,
                                                                                   1,
                                                                                   colorFormat,
                                                                                   sampleCount == TextureSampleCount.Count1
                                                                                       ? TextureUsage.RenderTarget | TextureUsage.Sampled
                                                                                       : TextureUsage.RenderTarget,
                                                                                   sampleCount));
        _depthTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(width,
                                                                                   height,
                                                                                   1,
                                                                                   depthFormat,
                                                                                   TextureUsage.DepthStencil,
                                                                                   sampleCount));
        _framebuffer = resourceFactory.CreateFramebuffer(new FramebufferDescription(_depthTexture, _colorTexture));
        _presentTexture = sampleCount == TextureSampleCount.Count1
            ? _colorTexture
            : resourceFactory.CreateTexture(TextureDescription.Texture2D(width,
                                                                         height,
                                                                         1,
                                                                         colorFormat,
                                                                         TextureUsage.Sampled,
                                                                         TextureSampleCount.Count1));
    }

    public uint Width => _width;

    public uint Height => _height;

    public PixelFormat ColorFormat => _colorFormat;

    public PixelFormat DepthFormat => _depthFormat;

    public TextureSampleCount SampleCount => _sampleCount;

    public Texture ColorTexture => _colorTexture;

    public Texture DepthTexture => _depthTexture;

    public Framebuffer Framebuffer => _framebuffer;

    public Texture PresentTexture => _presentTexture;

    public void Present(CommandList commandList)
    {
        if (_sampleCount == TextureSampleCount.Count1)
        {
            return;
        }

        commandList.ResolveTexture(_colorTexture, _presentTexture);
    }

    protected override void Destroy()
    {
        _colorTexture?.Dispose();
        _depthTexture?.Dispose();
        _framebuffer?.Dispose();
        _presentTexture?.Dispose();
    }
}
