using System.Runtime.CompilerServices;
using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Swapchain : DeviceResource
{
    private readonly SwapchainKHR? _swapchain;
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
                                                        &surfaceCapabilities).ThrowCode();

        uint surfaceFormatCount;
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   description.Target,
                                                   &surfaceFormatCount,
                                                   null).ThrowCode();

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   description.Target,
                                                   &surfaceFormatCount,
                                                   (SurfaceFormatKHR*)Unsafe.AsPointer(ref surfaceFormats[0])).ThrowCode();

        uint presentModeCount;
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        description.Target,
                                                        &presentModeCount,
                                                        null).ThrowCode();

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        description.Target,
                                                        &presentModeCount,
                                                        (PresentModeKHR*)Unsafe.AsPointer(ref presentModes[0])).ThrowCode();

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
        SwapchainExt.CreateSwapchain(Device, &createInfo, null, &swapchain).ThrowCode();

        uint imageCount;
        SwapchainExt.GetSwapchainImages(Device, swapchain, &imageCount, null).ThrowCode();

        VkImage[] images = new VkImage[imageCount];
        SwapchainExt.GetSwapchainImages(Device,
                                        swapchain,
                                        &imageCount,
                                        (VkImage*)Unsafe.AsPointer(ref images[0])).ThrowCode();

        Texture? depthBuffer = null;
        if (description.DepthFormat != null)
        {
            TextureDescription depthBufferDescription = new(description.Width,
                                                            description.Height,
                                                            1,
                                                            1,
                                                            description.DepthFormat.Value,
                                                            TextureUsage.DepthStencil,
                                                            TextureType.Texture2D,
                                                            TextureSampleCount.Count1);

            depthBuffer = new Texture(graphicsDevice, in depthBufferDescription);
        }

        Framebuffer[] framebuffers = new Framebuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            Texture colorBuffer = new(graphicsDevice,
                                      images[i],
                                      createInfo.ImageFormat,
                                      description.Width,
                                      description.Height);

            FramebufferDescription framebufferDescription = new(depthBuffer, colorBuffer);

            framebuffers[i] = new Framebuffer(graphicsDevice, in framebufferDescription, true);
        }

        _swapchain = swapchain;
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
