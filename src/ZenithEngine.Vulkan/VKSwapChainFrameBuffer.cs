using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKSwapChainFrameBuffer(GraphicsContext context,
                                             VKSwapChain swapChain) : GraphicsResource(context)
{
    private Texture? depthStencilTarget;
    private Texture[] colorTargets = [];
    private FrameBuffer[] frameBuffers = [];

    public FrameBuffer this[uint index] => frameBuffers[index];

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void CreateFrameBuffers(uint width, uint height, Format imageFormat)
    {
        DestroyFrameBuffers();

        bool hasDepthStencilAttachment = swapChain.Desc.DepthStencilTargetFormat is not null;

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

        if (hasDepthStencilAttachment)
        {
            TextureDesc desc = new(width,
                                   height,
                                   format: swapChain.Desc.DepthStencilTargetFormat!.Value,
                                   usage: TextureUsage.DepthStencil);

            depthStencilTarget = Context.Factory.CreateTexture(in desc);
        }

        colorTargets = new Texture[imageCount];
        frameBuffers = new FrameBuffer[imageCount];
        for (uint i = 0; i < imageCount; i++)
        {
            TextureDesc desc = new(width,
                                   height,
                                   format: VKFormats.GetPixelFormat(imageFormat),
                                   usage: TextureUsage.RenderTarget);

            colorTargets[i] = new VKTexture(Context, in desc, images[i]);

            FrameBufferDesc frameBufferDesc = FrameBufferDesc.New(hasDepthStencilAttachment ? new(depthStencilTarget!) : null,
                                                                  [new(colorTargets[i])]);

            frameBuffers[i] = Context.Factory.CreateFrameBuffer(in frameBufferDesc);
        }
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        DestroyFrameBuffers();
    }

    private void DestroyFrameBuffers()
    {
        foreach (FrameBuffer frameBuffer in frameBuffers)
        {
            frameBuffer.Dispose();
        }

        foreach (Texture colorTarget in colorTargets)
        {
            colorTarget.Dispose();
        }

        depthStencilTarget?.Dispose();

        depthStencilTarget = null;
        colorTargets = [];
        frameBuffers = [];
    }
}
