using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public readonly record struct SwapchainDescription
{
    public SwapchainDescription(SurfaceKHR target,
                                uint width,
                                uint height,
                                PixelFormat? depthFormat)
    {
        Target = target;
        Width = width;
        Height = height;
        DepthFormat = depthFormat;
    }

    /// <summary>
    /// The render target.
    /// </summary>
    public SurfaceKHR Target { get; init; }

    /// <summary>
    /// The width of the swapchain surface.
    /// </summary>
    public uint Width { get; init; }

    /// <summary>
    /// The height of the swapchain surface.
    /// </summary>
    public uint Height { get; init; }

    /// <summary>
    /// The optional format of the depth target of the Swapchain's Framebuffer.
    /// If null, the Swapchain will not have a depth target.
    /// </summary>
    public PixelFormat? DepthFormat { get; init; }
}
