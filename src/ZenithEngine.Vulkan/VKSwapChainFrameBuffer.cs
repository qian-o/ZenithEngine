using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKSwapChainFrameBuffer(GraphicsContext context,
                                             VKSwapChain swapChain) : GraphicsResource(context)
{
    private readonly VKFence fence = new(context);

    public uint CurrentIndex;

    private Texture? depthStencilTarget;
    private FrameBuffer[] frameBuffers = [];

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public FrameBuffer FrameBuffer => frameBuffers[CurrentIndex];

    public void InitFrameBuffers(uint width, uint height, Format imageFormat)
    {
        DestroyFrameBuffers();

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

            depthStencilTarget = Context.Factory.CreateTexture(ref desc);
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

            VKTexture colorTarget = new(Context, ref desc, images[i]);

            FrameBufferDesc frameBufferDesc = FrameBufferDesc.Default(depthStencilTarget is not null ? FrameBufferAttachmentDesc.Default(depthStencilTarget) : null,
                                                                      FrameBufferAttachmentDesc.Default(colorTarget));

            frameBuffers[i] = Context.Factory.CreateFrameBuffer(ref frameBufferDesc);
        }
    }

    public void DestroyFrameBuffers()
    {
        depthStencilTarget?.Dispose();

        foreach (FrameBuffer frameBuffer in frameBuffers)
        {
            frameBuffer.Dispose();
        }

        CurrentIndex = 0;

        depthStencilTarget = null;
        frameBuffers = [];
    }

    public void AcquireNextImage()
    {
        Context.KhrSwapchain!.AcquireNextImage(Context.Device,
                                               swapChain.Swapchain,
                                               ulong.MaxValue,
                                               default,
                                               fence.Fence,
                                               ref CurrentIndex);

        fence.Wait();
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        DestroyFrameBuffers();

        fence.Dispose();
    }
}
