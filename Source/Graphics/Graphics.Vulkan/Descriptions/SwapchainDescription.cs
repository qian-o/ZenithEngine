using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public struct SwapchainDescription(SurfaceKHR target,
                                   uint width,
                                   uint height,
                                   PixelFormat? depthFormat) : IEquatable<SwapchainDescription>
{
    /// <summary>
    /// The render target.
    /// </summary>
    public SurfaceKHR Target { get; set; } = target;

    /// <summary>
    /// The width of the swapchain surface.
    /// </summary>
    public uint Width { get; set; } = width;

    /// <summary>
    /// The height of the swapchain surface.
    /// </summary>
    public uint Height { get; set; } = height;

    /// <summary>
    /// The optional format of the depth target of the Swapchain's Framebuffer.
    /// If null, the Swapchain will not have a depth target.
    /// </summary>
    public PixelFormat? DepthFormat { get; set; } = depthFormat;

    public readonly bool Equals(SwapchainDescription other)
    {
        return Target.Handle == other.Target.Handle
               && Width == other.Width
               && Height == other.Height
               && DepthFormat == other.DepthFormat;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Target.GetHashCode(),
                                  Width.GetHashCode(),
                                  Height.GetHashCode(),
                                  DepthFormat.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is SwapchainDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Target: {Target}, Width: {Width}, Height: {Height}, DepthFormat: {DepthFormat}";
    }

    public static bool operator ==(SwapchainDescription left, SwapchainDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SwapchainDescription left, SwapchainDescription right)
    {
        return !(left == right);
    }
}
