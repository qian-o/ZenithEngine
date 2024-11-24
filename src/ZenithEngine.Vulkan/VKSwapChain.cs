using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKSwapChain : SwapChain
{
    private readonly VKFence fence;
    private readonly SurfaceKHR surface;

    private SwapchainKHR swapchain;
    private uint imageIndex;
    private Texture? depthStencilTarget;
    private FrameBuffer[] frameBuffers = [];

    public VKSwapChain(GraphicsContext context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
        fence = new(Context);
        surface = new(desc.Target.CreateSurfaceByVulkan(Context.Instance.Handle));

        InitSwapChain();
        AcquireNextImage();
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public override FrameBuffer FrameBuffer => frameBuffers[imageIndex];

    public override void Present()
    {
        fixed (SwapchainKHR* pSwapChain = &swapchain)
        fixed (uint* pImageIndex = &imageIndex)
        {
            PresentInfoKHR presentInfo = new()
            {
                SType = StructureType.PresentInfoKhr,
                SwapchainCount = 1,
                PSwapchains = pSwapChain,
                PImageIndices = pImageIndex
            };

            Result result = Context.KhrSwapchain!.QueuePresent(Context.DirectQueue, &presentInfo);

            if (result == Result.ErrorOutOfDateKhr)
            {
                return;
            }
            else
            {
                result.ThrowIfError();
            }
        }

        AcquireNextImage();
    }

    public override void Resize()
    {
        InitSwapChain();
        AcquireNextImage();
    }

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.SwapchainKhr, swapchain.Handle, name);
    }

    protected override void Destroy()
    {
        DestroySwapChain();
        fence.Dispose();

        Context.KhrSurface!.DestroySurface(Context.Instance, surface, null);
    }

    private void InitSwapChain()
    {
        DestroySwapChain();

        SurfaceCapabilitiesKHR capabilities;
        Context.KhrSurface!.GetPhysicalDeviceSurfaceCapabilities(Context.PhysicalDevice,
                                                                 surface,
                                                                 &capabilities);

        uint formatCount;
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           surface,
                                                           &formatCount,
                                                           null);

        SurfaceFormatKHR[] formats = new SurfaceFormatKHR[formatCount];
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           surface,
                                                           &formatCount,
                                                           out formats[0]);

        uint modeCount;
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                surface,
                                                                &modeCount,
                                                                null);

        PresentModeKHR[] modes = new PresentModeKHR[modeCount];
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                surface,
                                                                &modeCount,
                                                                out modes[0]);

        uint desiredImages = capabilities.MinImageCount + 1;
        if (capabilities.MaxImageCount > 0 && desiredImages > capabilities.MaxImageCount)
        {
            desiredImages = capabilities.MaxImageCount;
        }

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = surface,
            MinImageCount = desiredImages,
            ImageFormat = ChooseSwapSurfaceFormat(formats).Format,
            ImageColorSpace = ChooseSwapSurfaceFormat(formats).ColorSpace,
            ImageExtent = ChooseSwapExtent(capabilities),
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CompositeAlpha = capabilities.SupportedCompositeAlpha,
            PresentMode = ChooseSwapPresentMode(modes, Desc.VerticalSync),
            ImageSharingMode = SharingMode.Exclusive,
            Clipped = true
        };

        Context.KhrSwapchain!.CreateSwapchain(Context.Device,
                                              &createInfo,
                                              null,
                                              out swapchain).ThrowIfError();

        uint imageCount;
        Context.KhrSwapchain.GetSwapchainImages(Context.Device,
                                                swapchain,
                                                &imageCount,
                                                null).ThrowIfError();

        VkImage[] images = new VkImage[imageCount];
        Context.KhrSwapchain.GetSwapchainImages(Context.Device,
                                                swapchain,
                                                &imageCount,
                                                out images[0]).ThrowIfError();

        if (Desc.DepthStencilTargetFormat is not null)
        {
            TextureDesc desc = TextureDesc.Default(createInfo.ImageExtent.Width,
                                                   createInfo.ImageExtent.Height,
                                                   1,
                                                   1,
                                                   format: Desc.DepthStencilTargetFormat.Value,
                                                   usage: TextureUsage.DepthStencil);

            depthStencilTarget = Context.Factory.CreateTexture(ref desc);
        }

        frameBuffers = new FrameBuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            TextureDesc desc = TextureDesc.Default(createInfo.ImageExtent.Width,
                                                   createInfo.ImageExtent.Height,
                                                   1,
                                                   1,
                                                   format: VKFormats.GetPixelFormat(createInfo.ImageFormat),
                                                   usage: TextureUsage.RenderTarget);

            VKTexture colorTarget = new(Context, ref desc, images[i]);

            FrameBufferDesc frameBufferDesc = FrameBufferDesc.Default(depthStencilTarget is not null ? FrameBufferAttachmentDesc.Default(depthStencilTarget) : null,
                                                                      FrameBufferAttachmentDesc.Default(colorTarget));

            frameBuffers[i] = Context.Factory.CreateFrameBuffer(ref frameBufferDesc);
        }
    }

    private void DestroySwapChain()
    {
        if (swapchain.Handle == 0)
        {
            return;
        }

        foreach (FrameBuffer frameBuffer in frameBuffers)
        {
            frameBuffer.Dispose();
        }

        depthStencilTarget?.Dispose();

        Context.KhrSwapchain!.DestroySwapchain(Context.Device, swapchain, null);

        swapchain = default;
        imageIndex = 0;
        depthStencilTarget = null;
        frameBuffers = [];
    }

    private void AcquireNextImage()
    {
        Context.KhrSwapchain!.AcquireNextImage(Context.Device,
                                               swapchain,
                                               ulong.MaxValue,
                                               default,
                                               fence.Fence,
                                               ref imageIndex);

        fence.Wait();
    }

    private static SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] surfaceFormats)
    {
        foreach (SurfaceFormatKHR availableFormat in surfaceFormats)
        {
            if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                return availableFormat;
            }
        }

        return surfaceFormats[0];
    }

    private static PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] presentModes, bool vsync)
    {
        PresentModeKHR presentMode = PresentModeKHR.FifoKhr;

        if (vsync && presentModes.Contains(PresentModeKHR.FifoRelaxedKhr))
        {
            presentMode = PresentModeKHR.FifoRelaxedKhr;
        }
        else if (presentModes.Contains(PresentModeKHR.MailboxKhr))
        {
            presentMode = PresentModeKHR.MailboxKhr;
        }
        else if (presentModes.Contains(PresentModeKHR.ImmediateKhr))
        {
            presentMode = PresentModeKHR.ImmediateKhr;
        }

        return presentMode;
    }

    private static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }

        return new Extent2D((uint)Utils.Lerp(capabilities.MinImageExtent.Width,
                                             capabilities.MaxImageExtent.Width,
                                             0.5f),
                            (uint)Utils.Lerp(capabilities.MinImageExtent.Height,
                                             capabilities.MaxImageExtent.Height,
                                             0.5f));
    }
}
