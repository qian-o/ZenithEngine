using Graphics.Engine.Enums;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan.Helpers;

internal class Formats
{
    public static ImageType GetImageType(TextureType type)
    {
        return type switch
        {
            TextureType.Texture1D => ImageType.Type1D,
            TextureType.Texture2D or TextureType.TextureCube => ImageType.Type2D,
            TextureType.Texture3D => ImageType.Type3D,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static Format GetPixelFormat(PixelFormat pixelFormat, bool depthFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.R8UNorm => Format.R8Unorm,
            PixelFormat.R8SNorm => Format.R8SNorm,
            PixelFormat.R8UInt => Format.R8Uint,
            PixelFormat.R8SInt => Format.R8Sint,

            PixelFormat.R16UNorm => depthFormat ? Format.D16Unorm : Format.R16Unorm,
            PixelFormat.R16SNorm => Format.R16SNorm,
            PixelFormat.R16UInt => Format.R16Uint,
            PixelFormat.R16SInt => Format.R16Sint,
            PixelFormat.R16Float => Format.R16Sfloat,

            PixelFormat.R32UInt => Format.R32Uint,
            PixelFormat.R32SInt => Format.R32Sint,
            PixelFormat.R32Float => depthFormat ? Format.D32Sfloat : Format.R32Sfloat,

            PixelFormat.R8G8UNorm => Format.R8G8Unorm,
            PixelFormat.R8G8SNorm => Format.R8G8SNorm,
            PixelFormat.R8G8UInt => Format.R8G8Uint,
            PixelFormat.R8G8SInt => Format.R8G8Sint,

            PixelFormat.R16G16UNorm => Format.R16G16Unorm,
            PixelFormat.R16G16SNorm => Format.R16G16SNorm,
            PixelFormat.R16G16UInt => Format.R16G16Uint,
            PixelFormat.R16G16SInt => Format.R16G16Sint,
            PixelFormat.R16G16Float => Format.R16G16Sfloat,

            PixelFormat.R32G32UInt => Format.R32G32Uint,
            PixelFormat.R32G32SInt => Format.R32G32Sint,
            PixelFormat.R32G32Float => Format.R32G32Sfloat,

            PixelFormat.R8G8B8UNorm => Format.R8G8B8Unorm,
            PixelFormat.R8G8B8SNorm => Format.R8G8B8SNorm,
            PixelFormat.R8G8B8UInt => Format.R8G8B8Uint,
            PixelFormat.R8G8B8SInt => Format.R8G8B8Sint,

            PixelFormat.R16G16B16UNorm => Format.R16G16B16Unorm,
            PixelFormat.R16G16B16SNorm => Format.R16G16B16SNorm,
            PixelFormat.R16G16B16UInt => Format.R16G16B16Uint,
            PixelFormat.R16G16B16SInt => Format.R16G16B16Sint,
            PixelFormat.R16G16B16Float => Format.R16G16B16Sfloat,

            PixelFormat.R32G32B32UInt => Format.R32G32B32Uint,
            PixelFormat.R32G32B32SInt => Format.R32G32B32Sint,
            PixelFormat.R32G32B32Float => Format.R32G32B32Sfloat,

            PixelFormat.R8G8B8A8UNorm => Format.R8G8B8A8Unorm,
            PixelFormat.R8G8B8A8UNormSRgb => Format.R8G8B8A8Srgb,
            PixelFormat.R8G8B8A8SNorm => Format.R8G8B8A8SNorm,
            PixelFormat.R8G8B8A8UInt => Format.R8G8B8A8Uint,
            PixelFormat.R8G8B8A8SInt => Format.R8G8B8A8Sint,

            PixelFormat.R16G16B16A16UNorm => Format.R16G16B16A16Unorm,
            PixelFormat.R16G16B16A16SNorm => Format.R16G16B16A16SNorm,
            PixelFormat.R16G16B16A16UInt => Format.R16G16B16A16Uint,
            PixelFormat.R16G16B16A16SInt => Format.R16G16B16A16Sint,
            PixelFormat.R16G16B16A16Float => Format.R16G16B16A16Sfloat,

            PixelFormat.R32G32B32A32UInt => Format.R32G32B32A32Uint,
            PixelFormat.R32G32B32A32SInt => Format.R32G32B32A32Sint,
            PixelFormat.R32G32B32A32Float => Format.R32G32B32A32Sfloat,

            PixelFormat.B8G8R8A8UNorm => Format.B8G8R8A8Unorm,
            PixelFormat.B8G8R8A8UNormSRgb => Format.B8G8R8A8Srgb,

            PixelFormat.BC1RgbUNorm => Format.BC1RgbUnormBlock,
            PixelFormat.BC1RgbUNormSRgb => Format.BC1RgbSrgbBlock,
            PixelFormat.BC1RgbaUNorm => Format.BC1RgbaUnormBlock,
            PixelFormat.BC1RgbaUNormSRgb => Format.BC1RgbaSrgbBlock,
            PixelFormat.BC2UNorm => Format.BC2UnormBlock,
            PixelFormat.BC2UNormSRgb => Format.BC2SrgbBlock,
            PixelFormat.BC3UNorm => Format.BC3UnormBlock,
            PixelFormat.BC3UNormSRgb => Format.BC3SrgbBlock,
            PixelFormat.BC4UNorm => Format.BC4UnormBlock,
            PixelFormat.BC4SNorm => Format.BC4SNormBlock,
            PixelFormat.BC5UNorm => Format.BC5UnormBlock,
            PixelFormat.BC5SNorm => Format.BC5SNormBlock,
            PixelFormat.BC7UNorm => Format.BC7UnormBlock,
            PixelFormat.BC7UNormSRgb => Format.BC7SrgbBlock,

            PixelFormat.ETC2R8G8B8UNorm => Format.Etc2R8G8B8UnormBlock,
            PixelFormat.ETC2R8G8B8A1UNorm => Format.Etc2R8G8B8A1UnormBlock,
            PixelFormat.ETC2R8G8B8A8UNorm => Format.Etc2R8G8B8A8UnormBlock,

            PixelFormat.D16UNormS8UInt => Format.D16UnormS8Uint,
            PixelFormat.D24UNormS8UInt => Format.D24UnormS8Uint,
            PixelFormat.D32FloatS8UInt => Format.D32SfloatS8Uint,

            PixelFormat.R10G10B10A2UNorm => Format.A2B10G10R10UnormPack32,
            PixelFormat.R10G10B10A2UInt => Format.A2B10G10R10UintPack32,
            PixelFormat.R11G11B10Float => Format.B10G11R11UfloatPack32,

            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat))
        };
    }

    public static SampleCountFlags GetSampleCountFlags(TextureSampleCount sampleCount)
    {
        return sampleCount switch
        {
            TextureSampleCount.Count1 => SampleCountFlags.Count1Bit,
            TextureSampleCount.Count2 => SampleCountFlags.Count2Bit,
            TextureSampleCount.Count4 => SampleCountFlags.Count4Bit,
            TextureSampleCount.Count8 => SampleCountFlags.Count8Bit,
            TextureSampleCount.Count16 => SampleCountFlags.Count16Bit,
            TextureSampleCount.Count32 => SampleCountFlags.Count32Bit,
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount))
        };
    }

    public static ImageUsageFlags GetImageUsageFlags(TextureUsage usage)
    {
        ImageUsageFlags imageUsageFlags = ImageUsageFlags.TransferDstBit | ImageUsageFlags.TransferSrcBit;

        if (usage.HasFlag(TextureUsage.Sampled))
        {
            imageUsageFlags |= ImageUsageFlags.SampledBit;
        }

        if (usage.HasFlag(TextureUsage.Storage))
        {
            imageUsageFlags |= ImageUsageFlags.StorageBit;
        }

        if (usage.HasFlag(TextureUsage.RenderTarget))
        {
            imageUsageFlags |= ImageUsageFlags.ColorAttachmentBit;
        }

        if (usage.HasFlag(TextureUsage.DepthStencil))
        {
            imageUsageFlags |= ImageUsageFlags.DepthStencilAttachmentBit;
        }

        return imageUsageFlags;
    }

    public static ImageViewType GetImageViewType(TextureType type)
    {
        return type switch
        {
            TextureType.Texture1D => ImageViewType.Type1D,
            TextureType.Texture2D => ImageViewType.Type2D,
            TextureType.Texture3D => ImageViewType.Type3D,
            TextureType.TextureCube => ImageViewType.TypeCube,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}
