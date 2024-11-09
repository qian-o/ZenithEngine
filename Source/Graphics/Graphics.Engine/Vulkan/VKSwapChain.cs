using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Helpers;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKSwapChain : SwapChain
{
    private SwapchainKHR? swapchain;

    public VKSwapChain(Context context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
        Surface = desc.Target.Create<AllocationCallbacks>(Context.Instance.ToHandle(), null).ToSurface();

        InitSwapChain();
        AcquireNextImage();
    }

    public new VKContext Context => (VKContext)base.Context;

    public SurfaceKHR Surface { get; }

    public override void Present()
    {
        // TODO: Present

        AcquireNextImage();
    }

    public override void Resize()
    {
        InitSwapChain();
        AcquireNextImage();
    }

    protected override void SetName(string name)
    {
        if (swapchain != null)
        {
            Context.SetDebugName(ObjectType.SwapchainKhr, swapchain.Value.Handle, name);
        }
    }

    protected override void Destroy()
    {
        DestroySwapChain();
    }

    private void InitSwapChain()
    {
        DestroySwapChain();

        SurfaceCapabilitiesKHR surfaceCapabilities;
        Context.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(Context.PhysicalDevice,
                                                                Surface,
                                                                &surfaceCapabilities);

        uint surfaceFormatCount;
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           Surface,
                                                           &surfaceFormatCount,
                                                           null);

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           Surface,
                                                           &surfaceFormatCount,
                                                           surfaceFormats.AsPointer());

        uint presentModeCount;
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                Surface,
                                                                &presentModeCount,
                                                                null);

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                Surface,
                                                                &presentModeCount,
                                                                presentModes.AsPointer());

        uint desiredNumberOfSwapchainImages = surfaceCapabilities.MinImageCount + 1;
        if (surfaceCapabilities.MaxImageCount > 0 && desiredNumberOfSwapchainImages > surfaceCapabilities.MaxImageCount)
        {
            desiredNumberOfSwapchainImages = surfaceCapabilities.MaxImageCount;
        }

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = Surface,
            MinImageCount = desiredNumberOfSwapchainImages,
            ImageFormat = ChooseSwapSurfaceFormat(surfaceFormats).Format,
            ImageColorSpace = ChooseSwapSurfaceFormat(surfaceFormats).ColorSpace,
            ImageExtent = ChooseSwapExtent(surfaceCapabilities),
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CompositeAlpha = surfaceCapabilities.SupportedCompositeAlpha,
            PresentMode = ChooseSwapPresentMode(presentModes, Desc.VSync),
            ImageSharingMode = SharingMode.Exclusive,
            Clipped = true
        };

        SwapchainKHR swapchain;
        Context.KhrSwapchain.CreateSwapchain(Context.Device, &createInfo, null, &swapchain).ThrowCode();

        uint imageCount;
        Context.KhrSwapchain.GetSwapchainImages(Context.Device, swapchain, &imageCount, null);

        VkImage[] images = new VkImage[imageCount];
        Context.KhrSwapchain.GetSwapchainImages(Context.Device, swapchain, &imageCount, images.AsPointer());
    }

    private void DestroySwapChain()
    {
        if (swapchain != null)
        {
            Context.KhrSwapchain.DestroySwapchain(Context.Device, swapchain.Value, null);
        }
    }

    private void AcquireNextImage()
    {
        if (swapchain == null)
        {
            throw new InvalidOperationException("Swapchain is not initialized");
        }

        // TODO: Acquire next image
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

        return new Extent2D(Utils.Lerp(capabilities.MinImageExtent.Width,
                                       capabilities.MaxImageExtent.Width,
                                       0.5f),
                            Utils.Lerp(capabilities.MinImageExtent.Height,
                                       capabilities.MaxImageExtent.Height,
                                       0.5f));
    }
}
