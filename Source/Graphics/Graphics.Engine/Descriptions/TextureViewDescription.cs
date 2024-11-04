namespace Graphics.Engine.Descriptions;

public struct TextureViewDescription(Texture target,
                                     uint baseMipLevel,
                                     uint mipLevels)
{
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

    public static TextureViewDescription Create(Texture target)
    {
        return new TextureViewDescription(target, 0, target.Description.MipLevels);
    }
}
