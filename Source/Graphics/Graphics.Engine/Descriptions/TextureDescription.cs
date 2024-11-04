using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct TextureDescription(TextureType type,
                                 PixelFormat format,
                                 uint width,
                                 uint height,
                                 uint depth,
                                 uint mipLevels,
                                 TextureUsage usage,
                                 TextureSampleCount sampleCount)
{
    /// <summary>
    /// The type of the texture.
    /// </summary>
    public TextureType Type { get; set; } = type;

    /// <summary>
    /// The format of individual texture elements.
    /// </summary>
    public PixelFormat Format { get; set; } = format;

    /// <summary>
    /// The total width, in texels.
    /// </summary>
    public uint Width { get; set; } = width;

    /// <summary>
    /// The total height, in texels.
    /// </summary>
    public uint Height { get; set; } = height;

    /// <summary>
    /// The total depth, in texels.
    /// </summary>
    public uint Depth { get; set; } = depth;

    /// <summary>
    /// The number of mipmap levels.
    /// </summary>
    public uint MipLevels { get; set; } = mipLevels;

    /// <summary>
    /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader.
    /// </summary>
    public TextureUsage Usage { get; set; } = usage;

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; } = sampleCount;

    public static TextureDescription Create1D(uint width,
                                              uint mipLevels,
                                              PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new TextureDescription(TextureType.Texture1D,
                                      format,
                                      width,
                                      1,
                                      1,
                                      mipLevels,
                                      TextureUsage.Sampled,
                                      TextureSampleCount.Count1);
    }

    public static TextureDescription Create2D(uint width,
                                              uint height,
                                              uint mipLevels,
                                              PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new TextureDescription(TextureType.Texture2D,
                                      format,
                                      width,
                                      height,
                                      1,
                                      mipLevels,
                                      TextureUsage.Sampled,
                                      TextureSampleCount.Count1);
    }

    public static TextureDescription Create3D(uint width,
                                              uint height,
                                              uint depth,
                                              uint mipLevels,
                                              PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new TextureDescription(TextureType.Texture3D,
                                      format,
                                      width,
                                      height,
                                      depth,
                                      mipLevels,
                                      TextureUsage.Sampled,
                                      TextureSampleCount.Count1);
    }

    public static TextureDescription CreateCube(uint width,
                                                uint height,
                                                uint mipLevels,
                                                PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new TextureDescription(TextureType.TextureCube,
                                      format,
                                      width,
                                      height,
                                      1,
                                      mipLevels,
                                      TextureUsage.Sampled,
                                      TextureSampleCount.Count1);
    }
}
