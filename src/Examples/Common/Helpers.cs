using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using ZenithEngine.Common;

namespace Common;

internal static class Helpers
{
    public static Image<T>[] GenerateMipmaps<T>(Image<T> image,
                                                IResampler? resampler = null) where T : unmanaged, IPixel<T>
    {
        resampler ??= KnownResamplers.MitchellNetravali;

        Image<T>[] mipmaps = new Image<T>[Utils.GetMipLevels((uint)image.Width, (uint)image.Height)];

        mipmaps[0] = image;

        for (int i = 1; i < mipmaps.Length; i++)
        {
            Utils.GetMipDimensions((uint)image.Width,
                                   (uint)image.Height,
                                   (uint)i,
                                   out uint mipWidth,
                                   out uint mipHeight);

            mipmaps[i] = mipmaps[i - 1].Clone(context => context.Resize((int)mipWidth, (int)mipHeight, resampler));
        }

        return mipmaps;
    }
}
