﻿using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Swapchain : VulkanObject<SwapchainKHR>
{
    private SwapchainKHR? _swapchain;
    private Texture? _depthBuffer;
    private Framebuffer[]? _framebuffers;

    internal Swapchain(VulkanResources vkRes, ref readonly SwapchainDescription description) : base(vkRes, ObjectType.SwapchainKhr)
    {
        Target = description.Target.Create<AllocationCallbacks>(VkRes.Instance.ToHandle(), null).ToSurface();
        DepthFormat = description.DepthFormat;
        ImageAvailableFence = new Fence(vkRes);

        Resize(description.Width, description.Height);
    }

    internal override SwapchainKHR Handle => _swapchain ?? throw new InvalidOperationException("Swapchain is not initialized");

    internal SurfaceKHR Target { get; }

    internal PixelFormat? DepthFormat { get; }

    internal Fence ImageAvailableFence { get; }

    internal uint CurrentImageIndex { get; private set; }

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
        VkRes.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(VkRes.VkPhysicalDevice,
                                                              Target,
                                                              &surfaceCapabilities).ThrowCode();

        uint surfaceFormatCount;
        VkRes.KhrSurface.GetPhysicalDeviceSurfaceFormats(VkRes.VkPhysicalDevice,
                                                         Target,
                                                         &surfaceFormatCount,
                                                         null).ThrowCode();

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        VkRes.KhrSurface.GetPhysicalDeviceSurfaceFormats(VkRes.VkPhysicalDevice,
                                                         Target,
                                                         &surfaceFormatCount,
                                                         surfaceFormats.AsPointer()).ThrowCode();

        uint presentModeCount;
        VkRes.KhrSurface.GetPhysicalDeviceSurfacePresentModes(VkRes.VkPhysicalDevice,
                                                              Target,
                                                              &presentModeCount,
                                                              null).ThrowCode();

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        VkRes.KhrSurface.GetPhysicalDeviceSurfacePresentModes(VkRes.VkPhysicalDevice,
                                                              Target,
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
            Surface = Target,
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
        VkRes.KhrSwapchain.CreateSwapchain(VkRes.VkDevice, &createInfo, null, &swapchain).ThrowCode();

        uint imageCount;
        VkRes.KhrSwapchain.GetSwapchainImages(VkRes.VkDevice, swapchain, &imageCount, null).ThrowCode();

        VkImage[] images = new VkImage[imageCount];
        VkRes.KhrSwapchain.GetSwapchainImages(VkRes.VkDevice,
                                              swapchain,
                                              &imageCount,
                                              images.AsPointer()).ThrowCode();

        Texture? depthBuffer = null;
        if (DepthFormat != null)
        {
            TextureDescription depthBufferDescription = new(width,
                                                            height,
                                                            1,
                                                            1,
                                                            DepthFormat.Value,
                                                            TextureUsage.DepthStencil,
                                                            TextureType.Texture2D,
                                                            TextureSampleCount.Count1);

            depthBuffer = new Texture(VkRes, in depthBufferDescription);
        }

        Framebuffer[] framebuffers = new Framebuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            Texture colorBuffer = new(VkRes,
                                      images[i],
                                      createInfo.ImageFormat,
                                      width,
                                      height);

            FramebufferDescription framebufferDescription = new(depthBuffer, colorBuffer);

            framebuffers[i] = new Framebuffer(VkRes, in framebufferDescription, true);
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
        Result result = VkRes.KhrSwapchain.AcquireNextImage(VkRes.VkDevice,
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

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        if (_swapchain != null)
        {
            ImageAvailableFence.Dispose();

            DestroySwapchain();
        }

        VkRes.KhrSurface.DestroySurface(VkRes.Instance, Target, null);
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

            VkRes.KhrSwapchain.DestroySwapchain(VkRes.VkDevice, _swapchain.Value, null);
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
