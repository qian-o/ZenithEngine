using Graphics.Core;

namespace Graphics.Vulkan;

public struct FramebufferAttachmentDescription : IEquatable<FramebufferAttachmentDescription>
{
    public FramebufferAttachmentDescription(Texture target, uint arrayLayer, uint mipLevel)
    {
        if (arrayLayer >= target.ArrayLayers)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayLayer), "Array layer is out of range.");
        }

        if (mipLevel >= target.MipLevels)
        {
            throw new ArgumentOutOfRangeException(nameof(mipLevel), "Mip level is out of range.");
        }

        Target = target;
        ArrayLayer = arrayLayer;
        MipLevel = mipLevel;
    }

    public FramebufferAttachmentDescription(Texture target) : this(target, 0, 0)
    {
    }

    /// <summary>
    /// The target texture to render into.
    /// </summary>
    public Texture Target { get; set; }

    /// <summary>
    /// The array layer to render to.
    /// </summary>
    public uint ArrayLayer { get; set; }

    /// <summary>
    /// The mip level to render to.
    /// </summary>
    public uint MipLevel { get; set; }

    public readonly bool Equals(FramebufferAttachmentDescription other)
    {
        return Target == other.Target &&
               ArrayLayer == other.ArrayLayer &&
               MipLevel == other.MipLevel;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Target.GetHashCode(), ArrayLayer.GetHashCode(), MipLevel.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is FramebufferAttachmentDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Target: {Target}, ArrayLayer: {ArrayLayer}, MipLevel: {MipLevel}";
    }

    public static bool operator ==(FramebufferAttachmentDescription left, FramebufferAttachmentDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FramebufferAttachmentDescription left, FramebufferAttachmentDescription right)
    {
        return !(left == right);
    }
}
