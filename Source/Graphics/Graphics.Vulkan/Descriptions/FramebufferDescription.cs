using Graphics.Core;

namespace Graphics.Vulkan;

public struct FramebufferDescription(FramebufferAttachmentDescription colorTarget,
                                     FramebufferAttachmentDescription? depthTarget) : IEquatable<FramebufferDescription>
{
    public FramebufferDescription(Texture colorTarget,
                                  Texture depthTarget) : this(new FramebufferAttachmentDescription(colorTarget),
                                                              new FramebufferAttachmentDescription(depthTarget))
    {
    }

    /// <summary>
    /// The color target to render into.
    /// </summary>
    public FramebufferAttachmentDescription ColorTarget { get; set; } = colorTarget;

    /// <summary>
    /// The depth target to render into.
    /// </summary>
    public FramebufferAttachmentDescription? DepthTarget { get; set; } = depthTarget;

    public readonly bool Equals(FramebufferDescription other)
    {
        return ColorTarget == other.ColorTarget &&
               DepthTarget == other.DepthTarget;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(ColorTarget.GetHashCode(), DepthTarget?.GetHashCode() ?? 0);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is FramebufferDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"ColorTarget: {ColorTarget}, DepthTarget: {DepthTarget}";
    }

    public static bool operator ==(FramebufferDescription left, FramebufferDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FramebufferDescription left, FramebufferDescription right)
    {
        return !(left == right);
    }
}
