using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct TextureViewDescription
{
    public TextureViewDescription(Texture target,
                                  uint baseMipLevel,
                                  uint mipLevels,
                                  uint baseArrayLayer,
                                  uint arrayLayers,
                                  PixelFormat? format)
    {
        Target = target;
        BaseMipLevel = baseMipLevel;
        MipLevels = mipLevels;
        BaseArrayLayer = baseArrayLayer;
        ArrayLayers = arrayLayers;
        Format = format;
    }

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
    public Texture Target { get; }

    /// <summary>
    /// The base mip level visible in the view. Must be less than the number of mip levels in the target Texture.
    /// </summary>
    public uint BaseMipLevel { get; }

    /// <summary>
    /// The number of mip levels visible in the view.
    /// </summary>
    public uint MipLevels { get; }

    /// <summary>
    /// The base array layer visible in the view.
    /// </summary>
    public uint BaseArrayLayer { get; }

    /// <summary>
    /// The number of array layers visible in the view.
    /// </summary>
    public uint ArrayLayers { get; }

    /// <summary>
    /// The format of the view.
    /// </summary>
    public PixelFormat? Format { get; }
}
