using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.DirectX12;

internal static class DXFormats
{
    #region To DirectX12
    public static ResourceDimension GetResourceDimension(TextureType type)
    {
        return type switch
        {
            TextureType.Texture1D => ResourceDimension.Texture1D,
            TextureType.Texture2D or TextureType.TextureCube => ResourceDimension.Texture2D,
            TextureType.Texture3D => ResourceDimension.Texture3D,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type))
        };
    }

    public static Format GetFormat(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.R8UNorm => Format.FormatR8Unorm,
            PixelFormat.R8SNorm => Format.FormatR8SNorm,
            PixelFormat.R8UInt => Format.FormatR8Uint,
            PixelFormat.R8SInt => Format.FormatR8Sint,

            PixelFormat.R16UNorm => Format.FormatR16Unorm,
            PixelFormat.R16SNorm => Format.FormatR16SNorm,
            PixelFormat.R16UInt => Format.FormatR16Uint,
            PixelFormat.R16SInt => Format.FormatR16Sint,
            PixelFormat.R16Float => Format.FormatR16Float,

            PixelFormat.R32UInt => Format.FormatR32Uint,
            PixelFormat.R32SInt => Format.FormatR32Sint,
            PixelFormat.R32Float => Format.FormatR32Float,

            PixelFormat.R8G8UNorm => Format.FormatR8G8Unorm,
            PixelFormat.R8G8SNorm => Format.FormatR8G8SNorm,
            PixelFormat.R8G8UInt => Format.FormatR8G8Uint,
            PixelFormat.R8G8SInt => Format.FormatR8G8Sint,

            PixelFormat.R16G16UNorm => Format.FormatR16G16Unorm,
            PixelFormat.R16G16SNorm => Format.FormatR16G16SNorm,
            PixelFormat.R16G16UInt => Format.FormatR16G16Uint,
            PixelFormat.R16G16SInt => Format.FormatR16G16Sint,
            PixelFormat.R16G16Float => Format.FormatR16G16Float,

            PixelFormat.R32G32UInt => Format.FormatR32G32Uint,
            PixelFormat.R32G32SInt => Format.FormatR32G32Sint,
            PixelFormat.R32G32Float => Format.FormatR32G32Float,

            PixelFormat.R32G32B32UInt => Format.FormatR32G32B32Uint,
            PixelFormat.R32G32B32SInt => Format.FormatR32G32B32Sint,
            PixelFormat.R32G32B32Float => Format.FormatR32G32B32Float,

            PixelFormat.R8G8B8A8UNorm => Format.FormatR8G8B8A8Unorm,
            PixelFormat.R8G8B8A8UNormSRgb => Format.FormatR8G8B8A8UnormSrgb,
            PixelFormat.R8G8B8A8SNorm => Format.FormatR8G8B8A8SNorm,
            PixelFormat.R8G8B8A8UInt => Format.FormatR8G8B8A8Uint,
            PixelFormat.R8G8B8A8SInt => Format.FormatR8G8B8A8Sint,

            PixelFormat.R16G16B16A16UNorm => Format.FormatR16G16B16A16Unorm,
            PixelFormat.R16G16B16A16SNorm => Format.FormatR16G16B16A16SNorm,
            PixelFormat.R16G16B16A16UInt => Format.FormatR16G16B16A16Uint,
            PixelFormat.R16G16B16A16SInt => Format.FormatR16G16B16A16Sint,
            PixelFormat.R16G16B16A16Float => Format.FormatR16G16B16A16Float,

            PixelFormat.R32G32B32A32UInt => Format.FormatR32G32B32A32Uint,
            PixelFormat.R32G32B32A32SInt => Format.FormatR32G32B32A32Sint,
            PixelFormat.R32G32B32A32Float => Format.FormatR32G32B32A32Float,

            PixelFormat.B8G8R8A8UNorm => Format.FormatB8G8R8A8Unorm,
            PixelFormat.B8G8R8A8UNormSRgb => Format.FormatB8G8R8A8UnormSrgb,

            PixelFormat.BC1UNorm => Format.FormatBC1Unorm,
            PixelFormat.BC1UNormSRgb => Format.FormatBC1UnormSrgb,
            PixelFormat.BC2UNorm => Format.FormatBC2Unorm,
            PixelFormat.BC2UNormSRgb => Format.FormatBC2UnormSrgb,
            PixelFormat.BC3UNorm => Format.FormatBC3Unorm,
            PixelFormat.BC3UNormSRgb => Format.FormatBC3UnormSrgb,
            PixelFormat.BC4UNorm => Format.FormatBC4Unorm,
            PixelFormat.BC4SNorm => Format.FormatBC4SNorm,
            PixelFormat.BC5UNorm => Format.FormatBC5Unorm,
            PixelFormat.BC5SNorm => Format.FormatBC5SNorm,
            PixelFormat.BC7UNorm => Format.FormatBC7Unorm,
            PixelFormat.BC7UNormSRgb => Format.FormatBC7UnormSrgb,

            PixelFormat.D24UNormS8UInt => Format.FormatD24UnormS8Uint,
            PixelFormat.D32FloatS8UInt => Format.FormatD32FloatS8X24Uint,

            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(format))
        };
    }

    public static SampleDesc GetSampleDesc(TextureSampleCount count)
    {
        return count switch
        {
            TextureSampleCount.Count1 => new SampleDesc(1, 0),
            TextureSampleCount.Count2 => new SampleDesc(2, 0),
            TextureSampleCount.Count4 => new SampleDesc(4, 0),
            TextureSampleCount.Count8 => new SampleDesc(8, 0),
            TextureSampleCount.Count16 => new SampleDesc(16, 0),
            TextureSampleCount.Count32 => new SampleDesc(32, 0),
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(count))
        };
    }
    #endregion
}
