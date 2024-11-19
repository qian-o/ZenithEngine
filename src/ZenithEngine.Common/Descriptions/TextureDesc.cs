using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct TextureDesc
{
    /// <summary>
    /// The type of the texture.
    /// </summary>
    public TextureType Type { get; set; }

    /// <summary>
    /// The format of individual texture elements.
    /// </summary>
    public PixelFormat Format { get; set; }

    /// <summary>
    /// The total width, in texels.
    /// </summary>
    public uint Width { get; set; }

    /// <summary>
    /// The total height, in texels.
    /// </summary>
    public uint Height { get; set; }

    /// <summary>
    /// The total depth, in texels.
    /// </summary>
    public uint Depth { get; set; }

    /// <summary>
    /// The number of mipmap levels.
    /// </summary>
    public uint MipLevels { get; set; }

    /// <summary>
    /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader.
    /// </summary>
    public TextureUsage Usage { get; set; }

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; }

    public static TextureDesc Default(uint width,
                                      uint height,
                                      uint depth,
                                      uint mipLevels,
                                      TextureType type = TextureType.Texture2D,
                                      PixelFormat format = PixelFormat.R8G8B8A8UNorm,
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
            Usage = usage,
            SampleCount = sampleCount
        };
    }
}
