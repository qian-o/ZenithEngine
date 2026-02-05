using System;
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

    internal static MTLColorWriteMask GetMTLColorWriteMask(ColorWriteChannels channels)
    {
        MTLColorWriteMask mask = MTLColorWriteMask.None;

        if (channels.HasFlag(ColorWriteChannels.Red))
            mask |= MTLColorWriteMask.Red;
        if (channels.HasFlag(ColorWriteChannels.Green))
            mask |= MTLColorWriteMask.Green;
        if (channels.HasFlag(ColorWriteChannels.Blue))
            mask |= MTLColorWriteMask.Blue;
        if (channels.HasFlag(ColorWriteChannels.Alpha))
            mask |= MTLColorWriteMask.Alpha;

        return mask;
    }

    internal static MTLBlendFactor GetMTLBlendFactor(Blend blend)
    {
        return blend switch
        {
            Blend.Zero => MTLBlendFactor.Zero,
            Blend.One => MTLBlendFactor.One,
            Blend.SourceColor => MTLBlendFactor.SourceColor,
            Blend.InverseSourceColor => MTLBlendFactor.OneMinusSourceColor,
            Blend.SourceAlpha => MTLBlendFactor.SourceAlpha,
            Blend.InverseSourceAlpha => MTLBlendFactor.OneMinusSourceAlpha,
            Blend.DestinationColor => MTLBlendFactor.DestinationColor,
            Blend.InverseDestinationColor => MTLBlendFactor.OneMinusDestinationColor,
            Blend.DestinationAlpha => MTLBlendFactor.DestinationAlpha,
            Blend.InverseDestinationAlpha => MTLBlendFactor.OneMinusDestinationAlpha,
            Blend.BlendFactor => MTLBlendFactor.BlendColor,
            Blend.InverseBlendFactor => MTLBlendFactor.OneMinusBlendColor,
            _ => throw new NotSupportedException($"Blend factor {blend} is not supported.")
        };
    }

    internal static MTLBlendOperation GetMTLBlendOperation(BlendOperation operation)
    {
        return operation switch
        {
            BlendOperation.Add => MTLBlendOperation.Add,
            BlendOperation.Subtract => MTLBlendOperation.Subtract,
            BlendOperation.ReverseSubtract => MTLBlendOperation.ReverseSubtract,
            BlendOperation.Min => MTLBlendOperation.Min,
            BlendOperation.Max => MTLBlendOperation.Max,
            _ => throw new NotSupportedException($"Blend operation {operation} is not supported.")
        };
    }

    internal static MTLStencilOperation GetMTLStencilOperation(StencilOperation operation)
    {
        return operation switch
        {
            StencilOperation.Keep => MTLStencilOperation.Keep,
            StencilOperation.Zero => MTLStencilOperation.Zero,
            StencilOperation.Replace => MTLStencilOperation.Replace,
            StencilOperation.IncrementAndClamp => MTLStencilOperation.IncrementClamp,
            StencilOperation.DecrementAndClamp => MTLStencilOperation.DecrementClamp,
            StencilOperation.Invert => MTLStencilOperation.Invert,
            StencilOperation.IncrementAndWrap => MTLStencilOperation.IncrementWrap,
            StencilOperation.DecrementAndWrap => MTLStencilOperation.DecrementWrap,
            _ => throw new NotSupportedException($"Stencil operation {operation} is not supported.")
        };
    }

    internal static MTLVertexFormat GetMTLVertexFormat(ElementFormat format)
    {
        return format switch
        {
            ElementFormat.UByte1 => MTLVertexFormat.UChar,
            ElementFormat.UByte2 => MTLVertexFormat.UChar2,
            ElementFormat.UByte4 => MTLVertexFormat.UChar4,
            ElementFormat.Byte1 => MTLVertexFormat.Char,
            ElementFormat.Byte2 => MTLVertexFormat.Char2,
            ElementFormat.Byte4 => MTLVertexFormat.Char4,
            ElementFormat.UByte1Normalized => MTLVertexFormat.UCharNormalized,
            ElementFormat.UByte2Normalized => MTLVertexFormat.UChar2Normalized,
            ElementFormat.UByte4Normalized => MTLVertexFormat.UChar4Normalized,
            ElementFormat.Byte1Normalized => MTLVertexFormat.CharNormalized,
            ElementFormat.Byte2Normalized => MTLVertexFormat.Char2Normalized,
            ElementFormat.Byte4Normalized => MTLVertexFormat.Char4Normalized,
            ElementFormat.UShort1 => MTLVertexFormat.UShort,
            ElementFormat.UShort2 => MTLVertexFormat.UShort2,
            ElementFormat.UShort4 => MTLVertexFormat.UShort4,
            ElementFormat.Short1 => MTLVertexFormat.Short,
            ElementFormat.Short2 => MTLVertexFormat.Short2,
            ElementFormat.Short4 => MTLVertexFormat.Short4,
            ElementFormat.UShort1Normalized => MTLVertexFormat.UShortNormalized,
            ElementFormat.UShort2Normalized => MTLVertexFormat.UShort2Normalized,
            ElementFormat.UShort4Normalized => MTLVertexFormat.UShort4Normalized,
            ElementFormat.Short1Normalized => MTLVertexFormat.ShortNormalized,
            ElementFormat.Short2Normalized => MTLVertexFormat.Short2Normalized,
            ElementFormat.Short4Normalized => MTLVertexFormat.Short4Normalized,
            ElementFormat.Half1 => MTLVertexFormat.Half,
            ElementFormat.Half2 => MTLVertexFormat.Half2,
            ElementFormat.Half4 => MTLVertexFormat.Half4,
            ElementFormat.Float1 => MTLVertexFormat.Float,
            ElementFormat.Float2 => MTLVertexFormat.Float2,
            ElementFormat.Float3 => MTLVertexFormat.Float3,
            ElementFormat.Float4 => MTLVertexFormat.Float4,
            ElementFormat.UInt1 => MTLVertexFormat.UInt,
            ElementFormat.UInt2 => MTLVertexFormat.UInt2,
            ElementFormat.UInt3 => MTLVertexFormat.UInt3,
            ElementFormat.UInt4 => MTLVertexFormat.UInt4,
            ElementFormat.Int1 => MTLVertexFormat.Int,
            ElementFormat.Int2 => MTLVertexFormat.Int2,
            ElementFormat.Int3 => MTLVertexFormat.Int3,
            ElementFormat.Int4 => MTLVertexFormat.Int4,
            _ => throw new NotSupportedException($"Element format {format} is not supported.")
        };
    }

    internal static MTLVertexStepFunction GetMTLVertexStepFunction(VertexStepFunction stepFunction)
    {
        return stepFunction switch
        {
            VertexStepFunction.PerVertexData => MTLVertexStepFunction.PerVertex,
            VertexStepFunction.PerInstanceData => MTLVertexStepFunction.PerInstance,
            _ => throw new NotSupportedException($"Vertex step function {stepFunction} is not supported.")
        };
    }

    internal static MTLCullMode GetMTLCullMode(CullMode cullMode)
    {
        return cullMode switch
        {
            CullMode.None => MTLCullMode.None,
            CullMode.Front => MTLCullMode.Front,
            CullMode.Back => MTLCullMode.Back,
            _ => throw new NotSupportedException($"Cull mode {cullMode} is not supported.")
        };
    }

    internal static MTLWinding GetMTLWinding(FrontFace frontFace)
    {
        return frontFace switch
        {
            FrontFace.Clockwise => MTLWinding.Clockwise,
            FrontFace.CounterClockwise => MTLWinding.CounterClockwise,
            _ => throw new NotSupportedException($"Front face {frontFace} is not supported.")
        };
    }

    internal static MTLPrimitiveType GetMTLPrimitiveType(PrimitiveTopology topology)
    {
        return topology switch
        {
            PrimitiveTopology.PointList => MTLPrimitiveType.Point,
            PrimitiveTopology.LineList => MTLPrimitiveType.Line,
            PrimitiveTopology.LineStrip => MTLPrimitiveType.LineStrip,
            PrimitiveTopology.TriangleList => MTLPrimitiveType.Triangle,
            PrimitiveTopology.TriangleStrip => MTLPrimitiveType.TriangleStrip,
            _ => throw new NotSupportedException($"Primitive topology {topology} is not supported.")
        };
    }

    internal static MTLIndexType GetMTLIndexType(IndexFormat format)
    {
        return format switch
        {
            IndexFormat.UInt16 => MTLIndexType.UInt16,
            IndexFormat.UInt32 => MTLIndexType.UInt32,
            _ => throw new NotSupportedException($"Index format {format} is not supported.")
        };
    }
}
