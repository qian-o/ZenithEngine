using Graphics.Core;

namespace Graphics.Vulkan;

public struct TextureViewDescription(Texture target,
                                     uint baseMipLevel,
                                     uint mipLevels,
                                     uint baseArrayLayer,
                                     uint arrayLayers,
                                     PixelFormat? format) : IEquatable<TextureViewDescription>
{
    public TextureViewDescription(Texture target) : this(target,
                                                         0,
                                                         target.MipLevels,
                                                         0,
                                                         target.ArrayLayers,
                                                         target.Format)
    {
    }

    public TextureViewDescription(Texture target, uint baseMipLevel, uint baseArrayLayer) : this(target,
                                                                                                 baseMipLevel,
                                                                                                 target.MipLevels,
                                                                                                 baseArrayLayer,
                                                                                                 target.ArrayLayers,
                                                                                                 target.Format)
    {
    }

    public TextureViewDescription(Texture target, PixelFormat format) : this(target,
                                                                             0,
                                                                             target.MipLevels,
                                                                             0,
                                                                             target.ArrayLayers,
                                                                             format)
    {
    }

    /// <summary>
    /// The desired target.
    /// </summary>
    public Texture Target { get; set; } = target;

    /// <summary>
    /// The base mip level visible in the view. Must be less than the number of mip levels in the target Texture.
    /// </summary>
    public uint BaseMipLevel { get; set; } = baseMipLevel;

    /// <summary>
    /// The number of mip levels visible in the view.
    /// </summary>
    public uint MipLevels { get; set; } = mipLevels;

    /// <summary>
    /// The base array layer visible in the view.
    /// </summary>
    public uint BaseArrayLayer { get; set; } = baseArrayLayer;

    /// <summary>
    /// The number of array layers visible in the view.
    /// </summary>
    public uint ArrayLayers { get; set; } = arrayLayers;

    /// <summary>
    /// The format of the view.
    /// </summary>
    public PixelFormat? Format { get; set; } = format;

    public readonly bool Equals(TextureViewDescription other)
    {
        return Target == other.Target
               && BaseMipLevel == other.BaseMipLevel
               && MipLevels == other.MipLevels
               && BaseArrayLayer == other.BaseArrayLayer
               && ArrayLayers == other.ArrayLayers
               && Format == other.Format;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Target.GetHashCode(),
                                  BaseMipLevel.GetHashCode(),
                                  MipLevels.GetHashCode(),
                                  BaseArrayLayer.GetHashCode(),
                                  ArrayLayers.GetHashCode(),
                                  Format.GetHashCode());
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is TextureViewDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Target = {Target}, BaseMipLevel = {BaseMipLevel}, MipLevels = {MipLevels}, BaseArrayLayer = {BaseArrayLayer}, ArrayLayers = {ArrayLayers}, Format = {Format}";
    }

    public static bool operator ==(TextureViewDescription left, TextureViewDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextureViewDescription left, TextureViewDescription right)
    {
        return !(left == right);
    }
}
