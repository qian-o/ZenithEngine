using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKSwapChain : SwapChain
{
    private readonly SurfaceKHR surface;
    private readonly VKSwapChainFrameBuffer swapChainFrameBuffer;

    public SwapchainKHR Swapchain;

    public VKSwapChain(GraphicsContext context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
        surface = new(desc.Target.CreateSurfaceByVulkan(Context.Instance.Handle, (AllocationCallbacks*)null));
        swapChainFrameBuffer = new(Context, this);

        InitSwapChain();
        swapChainFrameBuffer.AcquireNextImage();
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public override FrameBuffer FrameBuffer => swapChainFrameBuffer.FrameBuffer;

    public override void Present()
    {
        fixed (SwapchainKHR* pSwapChain = &Swapchain)
        fixed (uint* pImageIndex = &swapChainFrameBuffer.CurrentIndex)
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

        swapChainFrameBuffer.AcquireNextImage();
    }

    public override void Resize()
    {
        InitSwapChain();
    }

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.SwapchainKhr, Swapchain.Handle, name);
    }

    protected override void Destroy()
    {
        DestroySwapChain();

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
                                              out Swapchain).ThrowIfError();

        swapChainFrameBuffer.AcquireNextImage();
    }

    private void DestroySwapChain()
    {
        if (Swapchain.Handle == 0)
        {
            return;
        }

        swapChainFrameBuffer.DestroyFrameBuffers();

        Context.KhrSwapchain!.DestroySwapchain(Context.Device, Swapchain, null);

        Swapchain = default;
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
