using Graphics.Core;

namespace Graphics.Vulkan;

public struct TextureDescription(uint width,
                                 uint height,
                                 uint depth,
                                 uint mipLevels,
                                 uint arrayLayers,
                                 PixelFormat format,
                                 TextureUsage usage,
                                 TextureType type,
                                 TextureSampleCount sampleCount) : IEquatable<TextureDescription>
{
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
    /// The number of array layers.
    /// </summary>
    public uint ArrayLayers { get; set; } = arrayLayers;

    /// <summary>
    /// The format of individual texture elements.
    /// </summary>
    public PixelFormat Format { get; set; } = format;

    /// <summary>
    /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader.
    /// </summary>
    public TextureUsage Usage { get; set; } = usage;

    /// <summary>
    /// The type of the texture.
    /// </summary>
    public TextureType Type { get; set; } = type;

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; } = sampleCount;

    public readonly bool Equals(TextureDescription other)
    {
        return Width == other.Width &&
               Height == other.Height &&
               Depth == other.Depth &&
               MipLevels == other.MipLevels &&
               ArrayLayers == other.ArrayLayers &&
               Format == other.Format &&
               Usage == other.Usage &&
               Type == other.Type &&
               SampleCount == other.SampleCount;
    }

    public override readonly int GetHashCode()
    {
        return HashHelper.Combine(Width.GetHashCode(),
                                  Height.GetHashCode(),
                                  Depth.GetHashCode(),
                                  MipLevels.GetHashCode(),
                                  ArrayLayers.GetHashCode(),
                                  (int)Format,
                                  (int)Usage,
                                  (int)Type,
                                  (int)SampleCount);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is TextureDescription description && Equals(description);
    }

    public override readonly string ToString()
    {
        return $"Width: {Width}, Height: {Height}, Depth: {Depth}, MipLevels: {MipLevels}, ArrayLayers: {ArrayLayers}, Format: {Format}, Usage: {Usage}, Type: {Type}, SampleCount: {SampleCount}";
    }

    public static bool operator ==(TextureDescription left, TextureDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextureDescription left, TextureDescription right)
    {
        return !(left == right);
    }

    public static TextureDescription Texture1D(uint width,
                                               uint mipLevels,
                                               uint arrayLayers,
                                               PixelFormat format,
                                               TextureUsage usage)
    {
        return new TextureDescription(width,
                                      1,
                                      1,
                                      mipLevels,
                                      arrayLayers,
                                      format,
                                      usage,
                                      TextureType.Texture1D,
                                      TextureSampleCount.Count1);
    }

    public static TextureDescription Texture2D(uint width,
                                               uint height,
                                               uint mipLevels,
                                               uint arrayLayers,
                                               PixelFormat format,
                                               TextureUsage usage)
    {
        return new TextureDescription(width,
                                      height,
                                      1,
                                      mipLevels,
                                      arrayLayers,
                                      format,
                                      usage,
                                      TextureType.Texture2D,
                                      TextureSampleCount.Count1);
    }

    public static TextureDescription Texture2D(uint width,
                                               uint height,
                                               uint mipLevels,
                                               uint arrayLayers,
                                               PixelFormat format,
                                               TextureUsage usage,
                                               TextureSampleCount sampleCount)
    {
        return new TextureDescription(width,
                                      height,
                                      1,
                                      mipLevels,
                                      arrayLayers,
                                      format,
                                      usage,
                                      TextureType.Texture2D,
                                      sampleCount);
    }

    public static TextureDescription Texture3D(uint width,
                                               uint height,
                                               uint depth,
                                               uint mipLevels,
                                               PixelFormat format,
                                               TextureUsage usage)
    {
        return new TextureDescription(width,
                                      height,
                                      depth,
                                      mipLevels,
                                      1,
                                      format,
                                      usage,
                                      TextureType.Texture3D,
                                      TextureSampleCount.Count1);
    }
}
