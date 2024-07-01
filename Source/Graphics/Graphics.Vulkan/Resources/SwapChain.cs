using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class SwapChain : DeviceResource
{
    private readonly SwapchainKHR? _swapchain;
    private readonly VkImage[]? _images;
    private readonly VkImageView[]? _imageViews;

    internal SwapChain(GraphicsDevice graphicsDevice, uint width, uint height) : base(graphicsDevice)
    {
        SurfaceCapabilitiesKHR surfaceCapabilities;
        SurfaceExt.GetPhysicalDeviceSurfaceCapabilities(VkPhysicalDevice,
                                                        WindowSurface,
                                                        &surfaceCapabilities);

        uint surfaceFormatCount;
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   WindowSurface,
                                                   &surfaceFormatCount,
                                                   null);

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   WindowSurface,
                                                   &surfaceFormatCount,
                                                   (SurfaceFormatKHR*)Unsafe.AsPointer(ref surfaceFormats[0]));

        uint presentModeCount;
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        WindowSurface,
                                                        &presentModeCount,
                                                        null);

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        WindowSurface,
                                                        &presentModeCount,
                                                        (PresentModeKHR*)Unsafe.AsPointer(ref presentModes[0]));

        if (width == 0 || height == 0)
        {
            return;
        }

        SwapchainCreateInfoKHR swapchainCreateInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = WindowSurface,
            MinImageCount = Math.Min(surfaceCapabilities.MinImageCount + 1, surfaceCapabilities.MaxImageCount),
            ImageFormat = ChooseSwapSurfaceFormat(surfaceFormats).Format,
            ImageColorSpace = ChooseSwapSurfaceFormat(surfaceFormats).ColorSpace,
            ImageExtent = ChooseSwapExtent(surfaceCapabilities, width, height),
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = ChooseSwapPresentMode(presentModes),
            ImageSharingMode = SharingMode.Exclusive,
            Clipped = true,
            OldSwapchain = default
        };

        SwapchainKHR swapchain;
        SwapchainExt.CreateSwapchain(Device, &swapchainCreateInfo, null, &swapchain);
        _swapchain = swapchain;

        uint imageCount;
        SwapchainExt.GetSwapchainImages(Device, swapchain, &imageCount, null);

        _images = new VkImage[imageCount];
        SwapchainExt.GetSwapchainImages(Device,
                                        swapchain,
                                        &imageCount,
                                        (VkImage*)Unsafe.AsPointer(ref _images[0]));

        _imageViews = new VkImageView[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            ImageViewCreateInfo imageViewCreateInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _images[i],
                ViewType = ImageViewType.Type2D,
                Format = ChooseSwapSurfaceFormat(surfaceFormats).Format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            VkImageView imageView;
            Vk.CreateImageView(Device, &imageViewCreateInfo, null, &imageView);
            _imageViews[i] = imageView;
        }
    }

    protected override void Destroy()
    {
        if (_imageViews != null)
        {
            foreach (VkImageView imageView in _imageViews)
            {
                Vk.DestroyImageView(Device, imageView, null);
            }
        }

        if (_swapchain != null)
        {
            SwapchainExt.DestroySwapchain(Device, _swapchain.Value, null);
        }
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

    private static PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] presentModes)
    {
        foreach (PresentModeKHR availablePresentMode in presentModes)
        {
            if (availablePresentMode == PresentModeKHR.MailboxKhr)
            {
                return availablePresentMode;
            }
        }

        return PresentModeKHR.FifoKhr;
    }

    private static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities, uint width, uint height)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }

        return new Extent2D(Math.Max(capabilities.MinImageExtent.Width, Math.Min(capabilities.MaxImageExtent.Width, width)),
                            Math.Max(capabilities.MinImageExtent.Height, Math.Min(capabilities.MaxImageExtent.Height, height)));
    }
}
