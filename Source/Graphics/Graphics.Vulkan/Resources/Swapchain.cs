using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Swapchain : DeviceResource
{
    private readonly SwapchainKHR? _swapchain;
    private readonly Texture? _colorBuffer;
    private readonly Texture? _depthBuffer;
    private readonly Framebuffer[]? _framebuffers;

    internal Swapchain(GraphicsDevice graphicsDevice, ref readonly SwapchainDescription description) : base(graphicsDevice)
    {
        if (description.Width == 0 || description.Height == 0)
        {
            return;
        }

        SurfaceCapabilitiesKHR surfaceCapabilities;
        SurfaceExt.GetPhysicalDeviceSurfaceCapabilities(VkPhysicalDevice,
                                                        description.Target,
                                                        &surfaceCapabilities);

        uint surfaceFormatCount;
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   description.Target,
                                                   &surfaceFormatCount,
                                                   null);

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   description.Target,
                                                   &surfaceFormatCount,
                                                   (SurfaceFormatKHR*)Unsafe.AsPointer(ref surfaceFormats[0]));

        uint presentModeCount;
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        description.Target,
                                                        &presentModeCount,
                                                        null);

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        description.Target,
                                                        &presentModeCount,
                                                        (PresentModeKHR*)Unsafe.AsPointer(ref presentModes[0]));

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = description.Target,
            MinImageCount = Math.Min(surfaceCapabilities.MinImageCount + 1, surfaceCapabilities.MaxImageCount),
            ImageFormat = ChooseSwapSurfaceFormat(surfaceFormats).Format,
            ImageColorSpace = ChooseSwapSurfaceFormat(surfaceFormats).ColorSpace,
            ImageExtent = ChooseSwapExtent(surfaceCapabilities, description.Width, description.Height),
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = ChooseSwapPresentMode(presentModes),
            ImageSharingMode = SharingMode.Exclusive,
            Clipped = Vk.True,
            OldSwapchain = default
        };

        SwapchainKHR swapchain;
        SwapchainExt.CreateSwapchain(Device, &createInfo, null, &swapchain);

        uint imageCount;
        SwapchainExt.GetSwapchainImages(Device, swapchain, &imageCount, null);

        VkImage[] images = new VkImage[imageCount];
        SwapchainExt.GetSwapchainImages(Device,
                                        swapchain,
                                        &imageCount,
                                        (VkImage*)Unsafe.AsPointer(ref images[0]));

        TextureDescription colorBufferDescription = new()
        {
            Width = description.Width,
            Height = description.Height,
            Depth = 1,
            MipLevels = 1,
            Format = Formats.GetPixelFormat(createInfo.ImageFormat),
            Usage = TextureUsage.RenderTarget,
            Type = TextureType.Texture2D,
            SampleCount = description.SampleCount
        };
        Texture colorBuffer = new(graphicsDevice, in colorBufferDescription);

        Texture? depthBuffer = null;
        if (description.DepthFormat != null)
        {
            TextureDescription depthBufferDescription = new()
            {
                Width = description.Width,
                Height = description.Height,
                Depth = 1,
                MipLevels = 1,
                Format = description.DepthFormat.Value,
                Usage = TextureUsage.DepthStencil,
                Type = TextureType.Texture2D,
                SampleCount = description.SampleCount
            };

            depthBuffer = new Texture(graphicsDevice, in depthBufferDescription);
        }

        Framebuffer[] framebuffers = new Framebuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            Texture resolveBuffer = new(graphicsDevice,
                                        images[i],
                                        createInfo.ImageFormat,
                                        description.Width,
                                        description.Height);

            FramebufferDescription framebufferDescription = new(colorBuffer, resolveBuffer, depthBuffer);

            framebuffers[i] = new Framebuffer(graphicsDevice, in framebufferDescription);
        }

        _swapchain = swapchain;
        _colorBuffer = colorBuffer;
        _depthBuffer = depthBuffer;
        _framebuffers = framebuffers;
    }

    protected override void Destroy()
    {
        if (_swapchain != null)
        {
            foreach (Framebuffer framebuffer in _framebuffers!)
            {
                framebuffer.Dispose();
            }

            _colorBuffer!.Dispose();
            _depthBuffer?.Dispose();

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
