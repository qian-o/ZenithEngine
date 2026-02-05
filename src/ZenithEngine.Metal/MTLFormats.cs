using Metal;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Metal;

internal static class MTLFormats
{
    public static MTLTextureType GetMTLTextureType(TextureType textureType)
    {
        return textureType switch
        {
            TextureType.Texture1D => MTLTextureType.k1D,
            TextureType.Texture1DArray => MTLTextureType.k1DArray,
            TextureType.Texture2D => MTLTextureType.k2D,
            TextureType.Texture2DArray => MTLTextureType.k2DArray,
            TextureType.Texture3D => MTLTextureType.k3D,
            TextureType.TextureCube => MTLTextureType.kCube,
            TextureType.TextureCubeArray => MTLTextureType.kCubeArray,
            _ => throw new NotSupportedException($"Texture type {textureType} is not supported in Metal.")
        };
    }

    internal static MTLPixelFormat GetMTLPixelFormat(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.R8UNorm => MTLPixelFormat.R8Unorm,
            PixelFormat.R8SNorm => MTLPixelFormat.R8Snorm,
            PixelFormat.R8UInt => MTLPixelFormat.R8Uint,
            PixelFormat.R8SInt => MTLPixelFormat.R8Sint,
            PixelFormat.R16UNorm => MTLPixelFormat.R16Unorm,
            PixelFormat.R16SNorm => MTLPixelFormat.R16Snorm,
            PixelFormat.R16UInt => MTLPixelFormat.R16Uint,
            PixelFormat.R16SInt => MTLPixelFormat.R16Sint,
            PixelFormat.R16Float => MTLPixelFormat.R16Float,
            PixelFormat.R32UInt => MTLPixelFormat.R32Uint,
            PixelFormat.R32SInt => MTLPixelFormat.R32Sint,
            PixelFormat.R32Float => MTLPixelFormat.R32Float,
            PixelFormat.R8G8UNorm => MTLPixelFormat.RG8Unorm,
            PixelFormat.R8G8SNorm => MTLPixelFormat.RG8Snorm,
            PixelFormat.R8G8UInt => MTLPixelFormat.RG8Uint,
            PixelFormat.R8G8SInt => MTLPixelFormat.RG8Sint,
            PixelFormat.R16G16UNorm => MTLPixelFormat.RG16Unorm,
            PixelFormat.R16G16SNorm => MTLPixelFormat.RG16Snorm,
            PixelFormat.R16G16UInt => MTLPixelFormat.RG16Uint,
            PixelFormat.R16G16SInt => MTLPixelFormat.RG16Sint,
            PixelFormat.R16G16Float => MTLPixelFormat.RG16Float,
            PixelFormat.R32G32UInt => MTLPixelFormat.RG32Uint,
            PixelFormat.R32G32SInt => MTLPixelFormat.RG32Sint,
            PixelFormat.R32G32Float => MTLPixelFormat.RG32Float,
            PixelFormat.R32G32B32UInt => MTLPixelFormat.Invalid,
            PixelFormat.R32G32B32SInt => MTLPixelFormat.Invalid,
            PixelFormat.R32G32B32Float => MTLPixelFormat.Invalid,
            PixelFormat.R8G8B8A8UNorm => MTLPixelFormat.RGBA8Unorm,
            PixelFormat.R8G8B8A8UNormSRgb => MTLPixelFormat.RGBA8Unorm_sRGB,
            PixelFormat.R8G8B8A8SNorm => MTLPixelFormat.RGBA8Snorm,
            PixelFormat.R8G8B8A8UInt => MTLPixelFormat.RGBA8Uint,
            PixelFormat.R8G8B8A8SInt => MTLPixelFormat.RGBA8Sint,
            PixelFormat.R16G16B16A16UNorm => MTLPixelFormat.RGBA16Unorm,
            PixelFormat.R16G16B16A16SNorm => MTLPixelFormat.RGBA16Snorm,
            PixelFormat.R16G16B16A16UInt => MTLPixelFormat.RGBA16Uint,
            PixelFormat.R16G16B16A16SInt => MTLPixelFormat.RGBA16Sint,
            PixelFormat.R16G16B16A16Float => MTLPixelFormat.RGBA16Float,
            PixelFormat.R32G32B32A32UInt => MTLPixelFormat.RGBA32Uint,
            PixelFormat.R32G32B32A32SInt => MTLPixelFormat.RGBA32Sint,
            PixelFormat.R32G32B32A32Float => MTLPixelFormat.RGBA32Float,
            PixelFormat.B8G8R8A8UNorm => MTLPixelFormat.BGRA8Unorm,
            PixelFormat.B8G8R8A8UNormSRgb => MTLPixelFormat.BGRA8Unorm_sRGB,
            PixelFormat.BC1UNorm => MTLPixelFormat.BC1RGBA,
            PixelFormat.BC1UNormSRgb => MTLPixelFormat.BC1_RGBA_sRGB,
            PixelFormat.BC2UNorm => MTLPixelFormat.BC2RGBA,
            PixelFormat.BC2UNormSRgb => MTLPixelFormat.BC2_RGBA_sRGB,
            PixelFormat.BC3UNorm => MTLPixelFormat.BC3RGBA,
            PixelFormat.BC3UNormSRgb => MTLPixelFormat.BC3_RGBA_sRGB,
            PixelFormat.BC4UNorm => MTLPixelFormat.BC4_RUnorm,
            PixelFormat.BC4SNorm => MTLPixelFormat.BC4_RSnorm,
            PixelFormat.BC5UNorm => MTLPixelFormat.BC5_RGUnorm,
            PixelFormat.BC5SNorm => MTLPixelFormat.BC5_RGSnorm,
            PixelFormat.BC7UNorm => MTLPixelFormat.BC7_RGBAUnorm,
            PixelFormat.BC7UNormSRgb => MTLPixelFormat.BC7_RGBAUnorm_sRGB,
            PixelFormat.D24UNormS8UInt => MTLPixelFormat.Depth24Unorm_Stencil8,
            PixelFormat.D32FloatS8UInt => MTLPixelFormat.Depth32Float_Stencil8,
            _ => throw new NotSupportedException($"Pixel format {format} is not supported in Metal.")
        };
    }

    /// <summary>
    /// Validates if a pixel format is supported by the given device.
    /// </summary>
    /// <param name="device">The Metal device to check.</param>
    /// <param name="format">The pixel format to validate.</param>
    /// <returns>True if the format is supported, false otherwise.</returns>
    internal static bool IsPixelFormatSupported(IMTLDevice device, PixelFormat format)
    {
        try
        {
            MTLPixelFormat mtlFormat = GetMTLPixelFormat(format);
            if (mtlFormat == MTLPixelFormat.Invalid)
            {
                return false;
            }

            // Check if the device supports the pixel format
            return device.SupportsTextureSampleCount(1, mtlFormat);
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    internal static MTLTextureUsage GetMTLTextureUsage(TextureUsage usage)
    {
        MTLTextureUsage mtlUsage = MTLTextureUsage.Unknown;

        if (usage.HasFlag(TextureUsage.ShaderResource))
        {
            mtlUsage |= MTLTextureUsage.ShaderRead;
        }

        if (usage.HasFlag(TextureUsage.UnorderedAccess))
        {
            mtlUsage |= MTLTextureUsage.ShaderRead | MTLTextureUsage.ShaderWrite;
        }

        if (usage.HasFlag(TextureUsage.RenderTarget))
        {
            mtlUsage |= MTLTextureUsage.RenderTarget;
        }

        return mtlUsage;
    }

    internal static nuint GetSampleCount(TextureSampleCount sampleCount)
    {
        return sampleCount switch
        {
            TextureSampleCount.Count1 => 1,
            TextureSampleCount.Count2 => 2,
            TextureSampleCount.Count4 => 4,
            TextureSampleCount.Count8 => 8,
            TextureSampleCount.Count16 => 16,
            TextureSampleCount.Count32 => 32,
            _ => throw new NotSupportedException($"Sample count {sampleCount} is not supported.")
        };
    }

    internal static MTLSamplerMinMagFilter GetMTLSamplerMinMagFilter(SamplerFilter filter)
    {
        return filter switch
        {
            SamplerFilter.MinPointMagPointMipPoint => MTLSamplerMinMagFilter.Nearest,
            SamplerFilter.MinPointMagPointMipLinear => MTLSamplerMinMagFilter.Nearest,
            SamplerFilter.MinPointMagLinearMipPoint => MTLSamplerMinMagFilter.Nearest,
            SamplerFilter.MinPointMagLinearMipLinear => MTLSamplerMinMagFilter.Nearest,
            SamplerFilter.MinLinearMagPointMipPoint => MTLSamplerMinMagFilter.Linear,
            SamplerFilter.MinLinearMagPointMipLinear => MTLSamplerMinMagFilter.Linear,
            SamplerFilter.MinLinearMagLinearMipPoint => MTLSamplerMinMagFilter.Linear,
            SamplerFilter.MinLinearMagLinearMipLinear => MTLSamplerMinMagFilter.Linear,
            SamplerFilter.Anisotropic => MTLSamplerMinMagFilter.Linear,
            _ => throw new NotSupportedException($"Sampler filter {filter} is not supported.")
        };
    }

    internal static MTLSamplerMipFilter GetMTLSamplerMipFilter(SamplerFilter filter)
    {
        return filter switch
        {
            SamplerFilter.MinPointMagPointMipPoint => MTLSamplerMipFilter.Nearest,
            SamplerFilter.MinPointMagPointMipLinear => MTLSamplerMipFilter.Linear,
            SamplerFilter.MinPointMagLinearMipPoint => MTLSamplerMipFilter.Nearest,
            SamplerFilter.MinPointMagLinearMipLinear => MTLSamplerMipFilter.Linear,
            SamplerFilter.MinLinearMagPointMipPoint => MTLSamplerMipFilter.Nearest,
            SamplerFilter.MinLinearMagPointMipLinear => MTLSamplerMipFilter.Linear,
            SamplerFilter.MinLinearMagLinearMipPoint => MTLSamplerMipFilter.Nearest,
            SamplerFilter.MinLinearMagLinearMipLinear => MTLSamplerMipFilter.Linear,
            SamplerFilter.Anisotropic => MTLSamplerMipFilter.Linear,
            _ => throw new NotSupportedException($"Sampler filter {filter} is not supported.")
        };
    }

    internal static MTLSamplerAddressMode GetMTLSamplerAddressMode(AddressMode addressMode)
    {
        return addressMode switch
        {
            AddressMode.Wrap => MTLSamplerAddressMode.Repeat,
            AddressMode.Mirror => MTLSamplerAddressMode.MirrorRepeat,
            AddressMode.Clamp => MTLSamplerAddressMode.ClampToEdge,
            AddressMode.Border => MTLSamplerAddressMode.ClampToBorderColor,
            _ => throw new NotSupportedException($"Address mode {addressMode} is not supported.")
        };
    }

    internal static MTLCompareFunction GetMTLCompareFunction(ComparisonFunction comparisonFunction)
    {
        return comparisonFunction switch
        {
            ComparisonFunction.Never => MTLCompareFunction.Never,
            ComparisonFunction.Less => MTLCompareFunction.Less,
            ComparisonFunction.Equal => MTLCompareFunction.Equal,
            ComparisonFunction.LessEqual => MTLCompareFunction.LessEqual,
            ComparisonFunction.Greater => MTLCompareFunction.Greater,
            ComparisonFunction.NotEqual => MTLCompareFunction.NotEqual,
            ComparisonFunction.GreaterEqual => MTLCompareFunction.GreaterEqual,
            ComparisonFunction.Always => MTLCompareFunction.Always,
            _ => throw new NotSupportedException($"Comparison function {comparisonFunction} is not supported.")
        };
    }
}
