using Graphics.Core;
using Graphics.Vulkan;

namespace Renderer.Components;

internal sealed class FBO(ResourceFactory resourceFactory,
                          PixelFormat colorFormat = PixelFormat.R8G8B8A8UNorm,
                          PixelFormat depthFormat = PixelFormat.D32FloatS8UInt,
                          TextureSampleCount sampleCount = TextureSampleCount.Count1) : DisposableObject
{
    public OutputDescription OutputDescription { get; } = new(new OutputAttachmentDescription(depthFormat), [new OutputAttachmentDescription(colorFormat)], sampleCount);

    public bool IsReady { get; private set; }

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public Texture? ColorTexture { get; private set; }

    public Texture? DepthTexture { get; private set; }

    public Framebuffer? Framebuffer { get; private set; }

    public Texture? PresentTexture { get; private set; }

    public bool Resize(uint width, uint height)
    {
        if (Width == width && Height == height)
        {
            return false;
        }

        Width = width;
        Height = height;

        ResetFramebuffer();

        return true;
    }

    public void Present(CommandList commandList)
    {
        if (!IsReady || sampleCount == TextureSampleCount.Count1)
        {
            return;
        }

        commandList.ResolveTexture(ColorTexture!, PresentTexture!);
    }

    protected override void Destroy()
    {
        PresentTexture?.Dispose();
        Framebuffer?.Dispose();
        DepthTexture?.Dispose();
        ColorTexture?.Dispose();
    }

    private void ResetFramebuffer()
    {
        IsReady = false;

        Destroy();

        if (Width == 0 || Height == 0)
        {
            return;
        }

        TextureUsage colorUsage = TextureUsage.RenderTarget;

        if (sampleCount == TextureSampleCount.Count1)
        {
            colorUsage |= TextureUsage.Sampled;
        }

        ColorTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(Width,
                                                                                  Height,
                                                                                  1,
                                                                                  colorFormat,
                                                                                  colorUsage,
                                                                                  sampleCount));

        DepthTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(Width,
                                                                                  Height,
                                                                                  1,
                                                                                  depthFormat,
                                                                                  TextureUsage.DepthStencil,
                                                                                  sampleCount));

        Framebuffer = resourceFactory.CreateFramebuffer(new FramebufferDescription(DepthTexture,
                                                                                   ColorTexture));

        if (sampleCount == TextureSampleCount.Count1)
        {
            PresentTexture = ColorTexture;
        }
        else
        {
            PresentTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(Width,
                                                                                        Height,
                                                                                        1,
                                                                                        colorFormat,
                                                                                        TextureUsage.Sampled,
                                                                                        TextureSampleCount.Count1));
        }

        IsReady = true;
    }
}
