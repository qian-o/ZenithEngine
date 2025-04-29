using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ZenithEngine.Common;

public static class Utils
{
    public const uint CbvCount = 32;

    public const uint SrvCount = 32;

    public const uint UavCount = 32;

    public const uint SmpCount = 16;

    public static uint GetMipLevels(uint width, uint height)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(width, height))) + 1;
    }

    public static uint GetMipLevels(uint width, uint height, uint depth)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(MathF.Max(width, height), depth))) + 1;
    }

    public static void GetMipDimensions(uint width,
                                        uint height,
                                        uint mipLevel,
                                        out uint mipWidth,
                                        out uint mipHeight)
    {
        mipWidth = Math.Max(1, width >> (int)mipLevel);
        mipHeight = Math.Max(1, height >> (int)mipLevel);
    }

    public static void GetMipDimensions(uint width,
                                        uint height,
                                        uint depth,
                                        uint mipLevel,
                                        out uint mipWidth,
                                        out uint mipHeight,
                                        out uint mipDepth)
    {
        mipWidth = Math.Max(1, width >> (int)mipLevel);
        mipHeight = Math.Max(1, height >> (int)mipLevel);
        mipDepth = Math.Max(1, depth >> (int)mipLevel);
    }

    public static Image<T>[] GenerateMipmaps<T>(Image<T> image,
                                                IResampler? resampler = null) where T : unmanaged, IPixel<T>
    {
        resampler ??= KnownResamplers.MitchellNetravali;

        Image<T>[] mipmaps = new Image<T>[GetMipLevels((uint)image.Width, (uint)image.Height)];

        mipmaps[0] = image;

        for (int i = 1; i < mipmaps.Length; i++)
        {
            GetMipDimensions((uint)image.Width,
                             (uint)image.Height,
                             (uint)i,
                             out uint mipWidth,
                             out uint mipHeight);

            mipmaps[i] = mipmaps[i - 1].Clone(context => context.Resize((int)mipWidth, (int)mipHeight, resampler));
        }

        return mipmaps;
    }

    public static T Align<T>(T size, T alignment) where T : INumberBase<T>, IBitwiseOperators<T, T, T>
    {
        return (size + alignment - T.One) & ~(alignment - T.One);
    }

    public static T Lerp<T>(T start, T end, T value) where T : INumberBase<T>
    {
        return start + ((end - start) * value);
    }

    public static T Clamp<T>(T value, T min, T max) where T : INumberBase<T>, IComparisonOperators<T, T, bool>
    {
        return value < min ? min : value > max ? max : value;
    }
}
