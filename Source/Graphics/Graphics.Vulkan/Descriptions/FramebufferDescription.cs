using Graphics.Core;

namespace Graphics.Vulkan;

public struct FramebufferDescription(FramebufferAttachmentDescription? depthTarget,
                                     params FramebufferAttachmentDescription[] colorTargets) : IEquatable<FramebufferDescription>
{
    public FramebufferDescription(Texture? depthTarget,
                                  params Texture[] colorTargets) : this(depthTarget != null ? new FramebufferAttachmentDescription(depthTarget) : null,
                                                                        colorTargets.Select(item => new FramebufferAttachmentDescription(item)).ToArray())
    {
    }

    /// <summary>
    /// The depth texture attachment.
    /// </summary>
    public FramebufferAttachmentDescription? DepthTarget { get; set; } = depthTarget;

    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FramebufferAttachmentDescription[] ColorTargets { get; set; } = colorTargets;

    public readonly bool Equals(FramebufferDescription other)
    {
        return DepthTarget == other.DepthTarget
               && ColorTargets.SequenceEqual(other.ColorTargets);
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(DepthTarget.GetHashCode(), ColorTargets.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is FramebufferDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"DepthTarget: {DepthTarget}, ColorTargets: {ColorTargets}";
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
