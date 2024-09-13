using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Swapchain : DeviceResource
{
    private readonly SurfaceKHR _target;
    private readonly PixelFormat? _depthFormat;
    private SwapchainKHR? _swapchain;
    private Texture? _depthBuffer;
    private Framebuffer[]? _framebuffers;

    internal Swapchain(GraphicsDevice graphicsDevice, ref readonly SwapchainDescription description) : base(graphicsDevice)
    {
        _target = description.Target.Create<AllocationCallbacks>(Instance.ToHandle(), null).ToSurface();
        _depthFormat = description.DepthFormat;
        ImageAvailableFence = new Fence(graphicsDevice);

        Resize(description.Width, description.Height);
    }

    internal SwapchainKHR Handle => _swapchain ?? throw new InvalidOperationException("Swapchain is not initialized");

    internal uint CurrentImageIndex { get; private set; }

    internal Fence ImageAvailableFence { get; }

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public Framebuffer Framebuffer => _framebuffers != null ? _framebuffers[CurrentImageIndex] : throw new InvalidOperationException("Swapchain is not initialized");

    public OutputDescription OutputDescription => Framebuffer.OutputDescription;

    public void Resize(uint width, uint height)
    {
        if (width == 0 || height == 0)
        {
            return;
        }

        DestroySwapchain();

        SurfaceCapabilitiesKHR surfaceCapabilities;
        SurfaceExt.GetPhysicalDeviceSurfaceCapabilities(VkPhysicalDevice,
                                                        _target,
                                                        &surfaceCapabilities).ThrowCode();

        uint surfaceFormatCount;
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   _target,
                                                   &surfaceFormatCount,
                                                   null).ThrowCode();

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        SurfaceExt.GetPhysicalDeviceSurfaceFormats(VkPhysicalDevice,
                                                   _target,
                                                   &surfaceFormatCount,
                                                   surfaceFormats.AsPointer()).ThrowCode();

        uint presentModeCount;
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        _target,
                                                        &presentModeCount,
                                                        null).ThrowCode();

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        SurfaceExt.GetPhysicalDeviceSurfacePresentModes(VkPhysicalDevice,
                                                        _target,
                                                        &presentModeCount,
                                                        presentModes.AsPointer()).ThrowCode();

        uint desiredNumberOfSwapchainImages = surfaceCapabilities.MinImageCount + 1;
        if (surfaceCapabilities.MaxImageCount > 0 && desiredNumberOfSwapchainImages > surfaceCapabilities.MaxImageCount)
        {
            desiredNumberOfSwapchainImages = surfaceCapabilities.MaxImageCount;
        }

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _target,
            MinImageCount = desiredNumberOfSwapchainImages,
            ImageFormat = ChooseSwapSurfaceFormat(surfaceFormats).Format,
            ImageColorSpace = ChooseSwapSurfaceFormat(surfaceFormats).ColorSpace,
            ImageExtent = ChooseSwapExtent(surfaceCapabilities, width, height),
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
                                        images.AsPointer()).ThrowCode();

        Texture? depthBuffer = null;
        if (_depthFormat != null)
        {
            TextureDescription depthBufferDescription = new(width,
                                                            height,
                                                            1,
                                                            1,
                                                            _depthFormat.Value,
                                                            TextureUsage.DepthStencil,
                                                            TextureType.Texture2D,
                                                            TextureSampleCount.Count1);

            depthBuffer = new Texture(GraphicsDevice, in depthBufferDescription);
        }

        Framebuffer[] framebuffers = new Framebuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            Texture colorBuffer = new(GraphicsDevice,
                                      images[i],
                                      createInfo.ImageFormat,
                                      width,
                                      height);

            FramebufferDescription framebufferDescription = new(depthBuffer, colorBuffer);

            framebuffers[i] = new Framebuffer(GraphicsDevice, in framebufferDescription, true);
        }

        _swapchain = swapchain;
        _depthBuffer = depthBuffer;
        _framebuffers = framebuffers;
        Width = width;
        Height = height;

        AcquireNextImage();
    }

    internal void AcquireNextImage(bool waitFence = true)
    {
        if (_swapchain == null)
        {
            throw new InvalidOperationException("Swapchain is not initialized");
        }

        uint currentImageIndex;
        Result result = SwapchainExt.AcquireNextImage(Device,
                                                      _swapchain.Value,
                                                      ulong.MaxValue,
                                                      default,
                                                      ImageAvailableFence.Handle,
                                                      &currentImageIndex);

        if (result is not Result.Success and not Result.SuboptimalKhr)
        {
            result.ThrowCode("Failed to acquire next image");
        }

        if (waitFence)
        {
            ImageAvailableFence.WaitAndReset();
        }

        CurrentImageIndex = currentImageIndex;
    }

    protected override void Destroy()
    {
        if (_swapchain != null)
        {
            ImageAvailableFence.Dispose();

            DestroySwapchain();
        }

        SurfaceExt.DestroySurface(Instance, _target, null);
    }

    private void DestroySwapchain()
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
