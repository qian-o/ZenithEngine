using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Swapchain : DeviceResource
{
    private readonly SurfaceKHR _target;
    private readonly PixelFormat? _depthFormat;
    private readonly Fence _imageAvailableFence;

    private SwapchainKHR? _swapchain;
    private Texture? _depthBuffer;
    private Framebuffer[]? _framebuffers;
    private uint _width;
    private uint _height;

    private uint _currentImageIndex;

    internal Swapchain(GraphicsDevice graphicsDevice, ref readonly SwapchainDescription description) : base(graphicsDevice)
    {
        _target = description.Target.Create<AllocationCallbacks>(Instance.ToHandle(), null).ToSurface();
        _depthFormat = description.DepthFormat;
        _imageAvailableFence = new Fence(graphicsDevice);

        Resize(description.Width, description.Height);
    }

    internal SwapchainKHR Handle => _swapchain ?? throw new InvalidOperationException("Swapchain is not initialized");

    internal uint CurrentImageIndex => _currentImageIndex;

    internal Fence ImageAvailableFence => _imageAvailableFence;

    public uint Width => _width;

    public uint Height => _height;

    public Framebuffer Framebuffer => _framebuffers != null ? _framebuffers[_currentImageIndex] : throw new InvalidOperationException("Swapchain is not initialized");

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

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _target,
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
        _width = width;
        _height = height;

        AcquireNextImage();
    }

    internal void AcquireNextImage(bool waitFence = true)
    {
        if (_swapchain == null)
        {
            throw new InvalidOperationException("Swapchain is not initialized");
        }

        uint currentImageIndex;
        SwapchainExt.AcquireNextImage(Device,
                                      _swapchain.Value,
                                      ulong.MaxValue,
                                      default,
                                      _imageAvailableFence.Handle,
                                      &currentImageIndex).ThrowCode();

        if (waitFence)
        {
            _imageAvailableFence.WaitAndReset();
        }

        _currentImageIndex = currentImageIndex;
    }

    protected override void Destroy()
    {
        if (_swapchain != null)
        {
            _imageAvailableFence.Dispose();

            DestroySwapchain();
        }
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
