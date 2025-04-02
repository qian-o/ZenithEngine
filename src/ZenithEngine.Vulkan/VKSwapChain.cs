using System.Diagnostics;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Vulkan;

internal unsafe partial class VKSwapChain : SwapChain
{
    [LibraryImport("android", EntryPoint = "ANativeWindow_fromSurface")]
    private static partial nint ANativeWindowFromSurface(nint env, nint surface);

    public SurfaceKHR Surface;
    public SwapchainKHR Swapchain;
    public uint ImageIndex;

    private readonly VKFence fence;
    private readonly VKSwapChainFrameBuffer swapChainFrameBuffer;

    public VKSwapChain(GraphicsContext context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
        fence = new(Context);
        swapChainFrameBuffer = new(Context, this);

        CreateSurface();
        CreateSwapChain();
        AcquireNextImage();
    }

    public override FrameBuffer FrameBuffer => swapChainFrameBuffer[ImageIndex];

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public override void Present()
    {
        fixed (SwapchainKHR* pSwapChain = &Swapchain)
        {
            fixed (uint* pImageIndex = &ImageIndex)
            {
                PresentInfoKHR presentInfo = new()
                {
                    SType = StructureType.PresentInfoKhr,
                    SwapchainCount = 1,
                    PSwapchains = pSwapChain,
                    PImageIndices = pImageIndex
                };

                Result result = Context.KhrSwapchain!.QueuePresent(Context.GraphicsQueue, &presentInfo);

                if (result is Result.ErrorOutOfDateKhr)
                {
                    return;
                }
                else
                {
                    result.ThrowIfError();
                }
            }
        }

        AcquireNextImage();
    }

    public override void Resize()
    {
        CreateSwapChain();
        AcquireNextImage();
    }

    public override void RefreshSurface(ISurface surface)
    {
        Desc.Surface = surface;

        CreateSurface();
        CreateSwapChain();
        AcquireNextImage();
    }

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.SwapchainKhr, Swapchain.Handle, name);
    }

    protected override void Destroy()
    {
        swapChainFrameBuffer.Dispose();
        fence.Dispose();

        DestroySwapChain();
        DestroySurface();
    }

    private void CreateSurface()
    {
        DestroySurface();

        switch (Desc.Surface.SurfaceType)
        {
            case SurfaceType.Win32:
                {
                    Win32SurfaceCreateInfoKHR createInfo = new()
                    {
                        SType = StructureType.Win32SurfaceCreateInfoKhr,
                        Hinstance = Process.GetCurrentProcess().Handle,
                        Hwnd = Desc.Surface.Handles[0]
                    };

                    Context.KhrWin32Surface!.CreateWin32Surface(Context.Instance,
                                                                &createInfo,
                                                                null,
                                                                out Surface).ThrowIfError();
                }
                break;
            case SurfaceType.Wayland:
                {
                    WaylandSurfaceCreateInfoKHR createInfo = new()
                    {
                        SType = StructureType.WaylandSurfaceCreateInfoKhr,
                        Display = (nint*)Desc.Surface.Handles[0],
                        Surface = (nint*)Desc.Surface.Handles[1]
                    };

                    Context.KhrWaylandSurface!.CreateWaylandSurface(Context.Instance,
                                                                    &createInfo,
                                                                    null,
                                                                    out Surface).ThrowIfError();
                }
                break;
            case SurfaceType.Xlib:
                {
                    XlibSurfaceCreateInfoKHR createInfo = new()
                    {
                        SType = StructureType.XlibSurfaceCreateInfoKhr,
                        Dpy = (nint*)Desc.Surface.Handles[0],
                        Window = Desc.Surface.Handles[1]
                    };

                    Context.KhrXlibSurface!.CreateXlibSurface(Context.Instance,
                                                              &createInfo,
                                                              null,
                                                              out Surface).ThrowIfError();
                }
                break;
            case SurfaceType.Android:
                {
                    AndroidSurfaceCreateInfoKHR createInfo = new()
                    {
                        SType = StructureType.AndroidSurfaceCreateInfoKhr,
                        Window = (nint*)ANativeWindowFromSurface(Desc.Surface.Handles[0], Desc.Surface.Handles[1])
                    };

                    Context.KhrAndroidSurface!.CreateAndroidSurface(Context.Instance,
                                                                    &createInfo,
                                                                    null,
                                                                    out Surface).ThrowIfError();
                }
                break;
            case SurfaceType.IOS:
                {
                    IOSSurfaceCreateInfoMVK createInfo = new()
                    {
                        SType = StructureType.IosSurfaceCreateInfoMvk,
                        PView = (void*)Desc.Surface.Handles[0]
                    };

                    Context.MvkIosSurface!.CreateIossurface(Context.Instance,
                                                            &createInfo,
                                                            null,
                                                            out Surface).ThrowIfError();
                }
                break;
            case SurfaceType.MacOS:
                {
                    MacOSSurfaceCreateInfoMVK createInfo = new()
                    {
                        SType = StructureType.MacosSurfaceCreateInfoMvk,
                        PView = (void*)Desc.Surface.Handles[0]
                    };

                    Context.MvkMacosSurface!.CreateMacOssurface(Context.Instance,
                                                                &createInfo,
                                                                null,
                                                                out Surface).ThrowIfError();
                }
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(Desc.Surface.SurfaceType));
        }
    }

    private void CreateSwapChain()
    {
        DestroySwapChain();

        SurfaceCapabilitiesKHR capabilities;
        Context.KhrSurface!.GetPhysicalDeviceSurfaceCapabilities(Context.PhysicalDevice,
                                                                 Surface,
                                                                 &capabilities).ThrowIfError();

        uint formatCount;
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           Surface,
                                                           &formatCount,
                                                           null).ThrowIfError();

        SurfaceFormatKHR[] formats = new SurfaceFormatKHR[formatCount];
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           Surface,
                                                           &formatCount,
                                                           out formats[0]).ThrowIfError();

        uint modeCount;
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                Surface,
                                                                &modeCount,
                                                                null).ThrowIfError();

        PresentModeKHR[] modes = new PresentModeKHR[modeCount];
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                Surface,
                                                                &modeCount,
                                                                out modes[0]).ThrowIfError();

        uint desiredImages = capabilities.MinImageCount + 1;
        if (capabilities.MaxImageCount > 0 && desiredImages > capabilities.MaxImageCount)
        {
            desiredImages = capabilities.MaxImageCount;
        }

        CompositeAlphaFlagsKHR compositeAlpha = CompositeAlphaFlagsKHR.InheritBitKhr;
        if (capabilities.SupportedCompositeAlpha.HasFlag(CompositeAlphaFlagsKHR.OpaqueBitKhr))
        {
            compositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
        }

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = Surface,
            MinImageCount = desiredImages,
            ImageFormat = ChooseSwapSurfaceFormat(formats).Format,
            ImageColorSpace = ChooseSwapSurfaceFormat(formats).ColorSpace,
            ImageExtent = ChooseSwapExtent(capabilities),
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CompositeAlpha = compositeAlpha,
            PresentMode = ChooseSwapPresentMode(modes),
            ImageSharingMode = SharingMode.Exclusive,
            Clipped = true
        };

        Context.KhrSwapchain!.CreateSwapchain(Context.Device,
                                              &createInfo,
                                              null,
                                              out Swapchain).ThrowIfError();

        swapChainFrameBuffer.CreateFrameBuffers(createInfo.ImageExtent.Width,
                                                createInfo.ImageExtent.Height);
    }

    private void AcquireNextImage()
    {
        Context.KhrSwapchain!.AcquireNextImage(Context.Device,
                                               Swapchain,
                                               ulong.MaxValue,
                                               default,
                                               fence.Fence,
                                               ref ImageIndex).ThrowIfError();

        fence.Wait();
    }

    private void DestroySurface()
    {
        if (Surface.Handle is 0)
        {
            return;
        }

        Context.KhrSurface!.DestroySurface(Context.Instance, Surface, null);
    }

    private void DestroySwapChain()
    {
        if (Swapchain.Handle is 0)
        {
            return;
        }

        swapChainFrameBuffer.DestroyFrameBuffers();

        Context.KhrSwapchain!.DestroySwapchain(Context.Device, Swapchain, null);
    }

    private SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] formats)
    {
        Format format = VKFormats.GetSwapChainFormat(Desc.ColorTargetFormat);

        formats = [.. formats.Where(item => item.Format == format).OrderBy(static item => item.ColorSpace)];

        if (formats.Length is 0)
        {
            throw new ZenithEngineException("Failed to find suitable surface format.");
        }

        return formats[0];
    }

    private PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] modes)
    {
        if (Desc.VerticalSync && modes.Contains(PresentModeKHR.FifoRelaxedKhr))
        {
            return PresentModeKHR.FifoRelaxedKhr;
        }
        else if (modes.Contains(PresentModeKHR.MailboxKhr))
        {
            return PresentModeKHR.MailboxKhr;
        }
        else if (modes.Contains(PresentModeKHR.ImmediateKhr))
        {
            return PresentModeKHR.ImmediateKhr;
        }

        return PresentModeKHR.FifoKhr;
    }

    private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
    {
        if (capabilities.CurrentExtent.Width is not uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }

        Vector2D<uint> size = Desc.Surface.GetSize();

        return new(Utils.Clamp(size.X,
                               capabilities.MinImageExtent.Width,
                               capabilities.MaxImageExtent.Width),
                   Utils.Clamp(size.Y,
                               capabilities.MinImageExtent.Height,
                               capabilities.MaxImageExtent.Height));
    }
}
