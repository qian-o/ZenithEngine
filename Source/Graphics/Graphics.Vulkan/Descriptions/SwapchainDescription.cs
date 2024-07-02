using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public struct SwapchainDescription(SurfaceKHR target,
                                   uint width,
                                   uint height,
                                   PixelFormat? depthFormat,
                                   TextureSampleCount sampleCount) : IEquatable<SwapchainDescription>
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

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; } = sampleCount;

    public readonly bool Equals(SwapchainDescription other)
    {
        return Target.Handle == other.Target.Handle &&
               Width == other.Width &&
               Height == other.Height &&
               DepthFormat == other.DepthFormat &&
               SampleCount == other.SampleCount;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Target.GetHashCode(),
                                  Width.GetHashCode(),
                                  Height.GetHashCode(),
                                  DepthFormat.GetHashCode(),
                                  SampleCount.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is SwapchainDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Target: {Target}, Width: {Width}, Height: {Height}, DepthFormat: {DepthFormat}, SampleCount: {SampleCount}";
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
