using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct TextureDesc(uint width,
                          uint height,
                          TextureType type = TextureType.Texture2D,
                          PixelFormat format = PixelFormat.R8G8B8A8UNorm,
                          uint depth = 1,
                          uint mipLevels = 1,
                          uint arrayLayers = 1,
                          TextureUsage usage = TextureUsage.Sampled,
                          TextureSampleCount sampleCount = TextureSampleCount.Count1)
{
    public TextureDesc() : this(0,
                                0,
                                TextureType.Texture2D,
                                PixelFormat.R8G8B8A8UNorm,
                                1,
                                1,
                                1,
                                TextureUsage.Sampled,
                                TextureSampleCount.Count1)
    {
    }

    /// <summary>
    /// The type of the texture.
    /// </summary>
    public TextureType Type = type;

    /// <summary>
    /// The format of individual texture elements.
    /// </summary>
    public PixelFormat Format = format;

    /// <summary>
    /// The total width, in texels.
    /// </summary>
    public uint Width = width;

    /// <summary>
    /// The total height, in texels.
    /// </summary>
    public uint Height = height;

    /// <summary>
    /// The total depth, in texels.
    /// </summary>
    public uint Depth = depth;

    /// <summary>
    /// The number of mipmap levels.
    /// </summary>
    public uint MipLevels = mipLevels;

    /// <summary>
    /// The number of array layers.
    /// </summary>
    public uint ArrayLayers = arrayLayers;

    /// <summary>
    /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader.
    /// </summary>
    public TextureUsage Usage = usage;

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount = sampleCount;
}
