using Graphics.Core;

namespace Graphics.Vulkan;

public struct FramebufferDescription(FramebufferAttachmentDescription colorTarget,
                                     FramebufferAttachmentDescription? resolveColorTarget,
                                     FramebufferAttachmentDescription? depthTarget) : IEquatable<FramebufferDescription>
{
    public FramebufferDescription(Texture colorTarget,
                                  Texture? resolveColorTarget,
                                  Texture? depthTarget) : this(new FramebufferAttachmentDescription(colorTarget),
                                                               resolveColorTarget != null ? new FramebufferAttachmentDescription(resolveColorTarget) : null,
                                                               depthTarget != null ? new FramebufferAttachmentDescription(depthTarget) : null)
    {
    }

    /// <summary>
    /// The color target to render into.
    /// </summary>
    public FramebufferAttachmentDescription ColorTarget { get; set; } = colorTarget;

    /// <summary>
    /// The color target to resolve into.
    /// </summary>
    public FramebufferAttachmentDescription? ResolveColorTarget { get; set; } = resolveColorTarget;

    /// <summary>
    /// The depth target to render into.
    /// </summary>
    public FramebufferAttachmentDescription? DepthTarget { get; set; } = depthTarget;

    public readonly bool Equals(FramebufferDescription other)
    {
        return ColorTarget == other.ColorTarget &&
               ResolveColorTarget == other.ResolveColorTarget &&
               DepthTarget == other.DepthTarget;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(ColorTarget.GetHashCode(),
                                  ResolveColorTarget.GetHashCode(),
                                  DepthTarget.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is FramebufferDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"ColorTarget: {ColorTarget}, ResolveColorTarget: {ResolveColorTarget}, DepthTarget: {DepthTarget}";
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
