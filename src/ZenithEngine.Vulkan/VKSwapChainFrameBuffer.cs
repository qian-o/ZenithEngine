using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKSwapChainFrameBuffer(GraphicsContext context,
                                             VKSwapChain swapChain) : GraphicsResource(context)
{
    public uint CurrentIndex;

    private Texture? depthStencilTarget;
    private FrameBuffer[] frameBuffers = [];

    public FrameBuffer FrameBuffer => frameBuffers[CurrentIndex];

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void CreateFrameBuffers(uint width, uint height, Format imageFormat)
    {
        uint imageCount;
        Context.KhrSwapchain!.GetSwapchainImages(Context.Device,
                                                 swapChain.Swapchain,
                                                 &imageCount,
                                                 null).ThrowIfError();

        VkImage[] images = new VkImage[imageCount];
        Context.KhrSwapchain.GetSwapchainImages(Context.Device,
                                                swapChain.Swapchain,
                                                &imageCount,
                                                out images[0]).ThrowIfError();

        if (swapChain.Desc.DepthStencilTargetFormat is not null)
        {
            TextureDesc desc = TextureDesc.Default(width,
                                                   height,
                                                   1,
                                                   1,
                                                   format: swapChain.Desc.DepthStencilTargetFormat.Value,
                                                   usage: TextureUsage.DepthStencil);

            depthStencilTarget = Context.Factory.CreateTexture(in desc);
        }

        frameBuffers = new FrameBuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            TextureDesc desc = TextureDesc.Default(width,
                                                   height,
                                                   1,
                                                   1,
                                                   format: VKFormats.GetPixelFormat(imageFormat),
                                                   usage: TextureUsage.RenderTarget);

            VKTexture colorTarget = new(Context, in desc, images[i]);

            FrameBufferDesc frameBufferDesc = FrameBufferDesc.Default(depthStencilTarget is not null ? FrameBufferAttachmentDesc.Default(depthStencilTarget) : null,
                                                                      FrameBufferAttachmentDesc.Default(colorTarget));

            frameBuffers[i] = Context.Factory.CreateFrameBuffer(in frameBufferDesc);
        }
    }

    public void DestroyFrameBuffers()
    {
        foreach (FrameBuffer frameBuffer in frameBuffers)
        {
            frameBuffer.Dispose();
        }

        depthStencilTarget?.Dispose();

        depthStencilTarget = null;
        frameBuffers = [];
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        DestroyFrameBuffers();
    }
}
