using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class SwapChain : ContextObject
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SurfaceCapabilitiesKHR _surfaceCapabilities;
    private readonly SurfaceFormatKHR _surfaceFormat;
    private readonly PresentModeKHR _presentMode;
    private readonly uint _minImageCount;

    private SwapchainKHR? _swapchain;
    private VkImage[]? _images;
    private VkImageView[]? _imageViews;

    internal SwapChain(Context context, GraphicsDevice graphicsDevice) : base(context)
    {
        SurfaceCapabilitiesKHR surfaceCapabilities;
        SurfaceExt.GetPhysicalDeviceSurfaceCapabilities(graphicsDevice.PhysicalDevice.VkPhysicalDevice,
                                                        graphicsDevice.WindowSurface,
                                                        &surfaceCapabilities);

        uint surfaceFormatCount;
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(graphicsDevice.PhysicalDevice.VkPhysicalDevice,
                                                   graphicsDevice.WindowSurface,
                                                   &surfaceFormatCount,
                                                   null);

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(graphicsDevice.PhysicalDevice.VkPhysicalDevice,
                                                   graphicsDevice.WindowSurface,
                                                   &surfaceFormatCount,
                                                   (SurfaceFormatKHR*)Unsafe.AsPointer(ref surfaceFormats[0]));

        uint presentModeCount;
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(graphicsDevice.PhysicalDevice.VkPhysicalDevice,
                                                        graphicsDevice.WindowSurface,
                                                        &presentModeCount,
                                                        null);

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(graphicsDevice.PhysicalDevice.VkPhysicalDevice,
                                                        graphicsDevice.WindowSurface,
                                                        &presentModeCount,
                                                        (PresentModeKHR*)Unsafe.AsPointer(ref presentModes[0]));

        _graphicsDevice = graphicsDevice;
        _surfaceCapabilities = surfaceCapabilities;
        _surfaceFormat = ChooseSwapSurfaceFormat(surfaceFormats);
        _presentMode = ChooseSwapPresentMode(presentModes);
        _minImageCount = Math.Min(surfaceCapabilities.MinImageCount + 1, surfaceCapabilities.MaxImageCount);
    }

    internal void Initialize(uint width, uint height)
    {
        Cleanup();

        if (width == 0 || height == 0)
        {
            return;
        }

        Extent2D extent = ChooseSwapExtent(_surfaceCapabilities, width, height);

        SwapchainCreateInfoKHR swapchainCreateInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _graphicsDevice.WindowSurface,
            MinImageCount = _minImageCount,
            ImageFormat = _surfaceFormat.Format,
            ImageColorSpace = _surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = _presentMode,
            ImageSharingMode = SharingMode.Exclusive,
            Clipped = true,
            OldSwapchain = default
        };

        SwapchainKHR swapchain;
        _graphicsDevice.SwapchainExt.CreateSwapchain(_graphicsDevice.Device, &swapchainCreateInfo, null, &swapchain);
        _swapchain = swapchain;

        uint imageCount;
        _graphicsDevice.SwapchainExt.GetSwapchainImages(_graphicsDevice.Device, swapchain, &imageCount, null);

        _images = new VkImage[imageCount];
        _graphicsDevice.SwapchainExt.GetSwapchainImages(_graphicsDevice.Device,
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
                Format = _surfaceFormat.Format,
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
            Vk.CreateImageView(_graphicsDevice.Device, &imageViewCreateInfo, null, &imageView);
            _imageViews[i] = imageView;
        }
    }

    internal void Cleanup()
    {
        if (_imageViews != null)
        {
            foreach (VkImageView imageView in _imageViews)
            {
                Vk.DestroyImageView(_graphicsDevice.Device, imageView, null);
            }
        }

        if (_swapchain != null)
        {
            _graphicsDevice.SwapchainExt.DestroySwapchain(_graphicsDevice.Device, _swapchain.Value, null);
        }

        _imageViews = null;
        _images = null;
        _swapchain = null;
    }

    protected override void Destroy()
    {
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
