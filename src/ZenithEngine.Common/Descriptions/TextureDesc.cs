using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct TextureDesc
{
    /// <summary>
    /// The type of the texture.
    /// </summary>
    public TextureType Type;

    /// <summary>
    /// The format of individual texture elements.
    /// </summary>
    public PixelFormat Format;

    /// <summary>
    /// The total width, in texels.
    /// </summary>
    public uint Width;

    /// <summary>
    /// The total height, in texels.
    /// </summary>
    public uint Height;

    /// <summary>
    /// The total depth, in texels.
    /// </summary>
    public uint Depth;

    /// <summary>
    /// The number of mipmap levels.
    /// </summary>
    public uint MipLevels;

    /// <summary>
    /// The number of array layers.
    /// </summary>
    public uint ArrayLayers;

    /// <summary>
    /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader.
    /// </summary>
    public TextureUsage Usage;

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount;

    public static TextureDesc Default(uint width,
                                      uint height,
                                      TextureType type = TextureType.Texture2D,
                                      PixelFormat format = PixelFormat.R8G8B8A8UNorm,
                                      uint depth = 1,
                                      uint mipLevels = 1,
                                      uint arrayLayers = 1,
                                      TextureUsage usage = TextureUsage.Sampled,
                                      TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        return new()
        {
            Type = type,
            Format = format,
            Width = width,
            Height = height,
            Depth = depth,
            MipLevels = mipLevels,
            ArrayLayers = arrayLayers,
            Usage = usage,
            SampleCount = sampleCount
        };
    }
}
