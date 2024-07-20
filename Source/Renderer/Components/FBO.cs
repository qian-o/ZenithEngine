using Graphics.Core;
using Graphics.Vulkan;

namespace Renderer.Components;

internal sealed class FBO(ResourceFactory resourceFactory) : DisposableObject
{
    public bool IsReady { get; private set; }

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public TextureSampleCount SampleCount { get; private set; }

    public Texture? ColorTexture { get; private set; }

    public Texture? DepthTexture { get; private set; }

    public Framebuffer? Framebuffer { get; private set; }

    public Texture? PresentTexture { get; private set; }

    public bool Update(uint width, uint height, TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        if (Width == width && Height == height && SampleCount == sampleCount)
        {
            return false;
        }

        Width = width;
        Height = height;
        SampleCount = sampleCount;

        ResetFramebuffer();

        return true;
    }

    public void Present(CommandList commandList)
    {
        if (!IsReady || SampleCount == TextureSampleCount.Count1)
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

        if (SampleCount == TextureSampleCount.Count1)
        {
            colorUsage |= TextureUsage.Sampled;
        }

        ColorTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(Width,
                                                                                  Height,
                                                                                  1,
                                                                                  PixelFormat.R8G8B8A8UNorm,
                                                                                  colorUsage,
                                                                                  SampleCount));

        DepthTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(Width,
                                                                                  Height,
                                                                                  1,
                                                                                  PixelFormat.D32FloatS8UInt,
                                                                                  TextureUsage.DepthStencil,
                                                                                  SampleCount));

        Framebuffer = resourceFactory.CreateFramebuffer(new FramebufferDescription(DepthTexture,
                                                                                   ColorTexture));

        if (SampleCount == TextureSampleCount.Count1)
        {
            PresentTexture = ColorTexture;
        }
        else
        {
            PresentTexture = resourceFactory.CreateTexture(TextureDescription.Texture2D(Width,
                                                                                        Height,
                                                                                        1,
                                                                                        PixelFormat.R8G8B8A8UNorm,
                                                                                        TextureUsage.Sampled,
                                                                                        TextureSampleCount.Count1));
        }

        IsReady = true;
    }
}
