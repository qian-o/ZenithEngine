﻿using Graphics.Core;

namespace Graphics.Vulkan;

public readonly record struct TextureDescription
{
    public TextureDescription(uint width,
                              uint height,
                              uint depth,
                              uint mipLevels,
                              PixelFormat format,
                              TextureUsage usage,
                              TextureType type,
                              TextureSampleCount sampleCount)
    {
        Width = width;
        Height = height;
        Depth = depth;
        MipLevels = mipLevels;
        Format = format;
        Usage = usage;
        Type = type;
        SampleCount = sampleCount;
    }

    /// <summary>
    /// The total width, in texels.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// The total height, in texels.
    /// </summary>
    public uint Height { get; }

    /// <summary>
    /// The total depth, in texels.
    /// </summary>
    public uint Depth { get; }

    /// <summary>
    /// The number of mipmap levels.
    /// </summary>
    public uint MipLevels { get; }

    /// <summary>
    /// The format of individual texture elements.
    /// </summary>
    public PixelFormat Format { get; }

    /// <summary>
    /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader.
    /// </summary>
    public TextureUsage Usage { get; }

    /// <summary>
    /// The type of the texture.
    /// </summary>
    public TextureType Type { get; }

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount { get; }

    public static TextureDescription Texture1D(uint width,
                                               uint mipLevels,
                                               PixelFormat format,
                                               TextureUsage usage)
    {
        return new TextureDescription(width,
                                      1,
                                      1,
                                      mipLevels,
                                      format,
                                      usage,
                                      TextureType.Texture1D,
                                      TextureSampleCount.Count1);
    }

    public static TextureDescription Texture2D(uint width,
                                               uint height,
                                               uint mipLevels,
                                               PixelFormat format,
                                               TextureUsage usage)
    {
        return new TextureDescription(width,
                                      height,
                                      1,
                                      mipLevels,
                                      format,
                                      usage,
                                      TextureType.Texture2D,
                                      TextureSampleCount.Count1);
    }

    public static TextureDescription Texture2D(uint width,
                                               uint height,
                                               uint mipLevels,
                                               PixelFormat format,
                                               TextureUsage usage,
                                               TextureSampleCount sampleCount)
    {
        return new TextureDescription(width,
                                      height,
                                      1,
                                      mipLevels,
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
                                      format,
                                      usage,
                                      TextureType.Texture3D,
                                      TextureSampleCount.Count1);
    }
}
