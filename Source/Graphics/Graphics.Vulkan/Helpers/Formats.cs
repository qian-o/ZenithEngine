using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal static class Formats
{
    #region To Vulkan Format
    public static Format GetPixelFormat(PixelFormat pixelFormat, bool depthFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.R8UNorm => Format.R8Unorm,
            PixelFormat.R8SNorm => Format.R8SNorm,
            PixelFormat.R8SInt => Format.R8Sint,

            PixelFormat.R16UNorm => Format.R16Unorm,
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

            PixelFormat.R8G8B8A8UNorm => Format.R8G8B8A8Unorm,
            PixelFormat.R8G8B8A8UNormSRgb => Format.R8G8B8A8Srgb,
            PixelFormat.B8G8R8A8UNorm => Format.B8G8R8A8Unorm,
            PixelFormat.B8G8R8A8UNormSRgb => Format.B8G8R8A8Srgb,
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

            PixelFormat.D32FloatS8UInt => Format.D32SfloatS8Uint,
            PixelFormat.D24UNormS8UInt => Format.D24UnormS8Uint,

            PixelFormat.R10G10B10A2UNorm => Format.A2B10G10R10UnormPack32,
            PixelFormat.R10G10B10A2UInt => Format.A2B10G10R10UintPack32,
            PixelFormat.R11G11B10Float => Format.B10G11R11UfloatPack32,

            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat))
        };
    }

    public static ImageType GetImageType(TextureType textureType)
    {
        return textureType switch
        {
            TextureType.Texture1D => ImageType.Type1D,
            TextureType.Texture2D => ImageType.Type2D,
            TextureType.Texture3D => ImageType.Type3D,
            _ => throw new ArgumentOutOfRangeException(nameof(textureType))
        };
    }

    public static ImageUsageFlags GetImageUsageFlags(TextureUsage textureUsage)
    {
        ImageUsageFlags imageUsageFlags = ImageUsageFlags.TransferDstBit | ImageUsageFlags.TransferSrcBit;

        if (textureUsage.HasFlag(TextureUsage.RenderTarget))
        {
            imageUsageFlags |= ImageUsageFlags.ColorAttachmentBit;
        }

        if (textureUsage.HasFlag(TextureUsage.DepthStencil))
        {
            imageUsageFlags |= ImageUsageFlags.DepthStencilAttachmentBit;
        }

        if (textureUsage.HasFlag(TextureUsage.Sampled))
        {
            imageUsageFlags |= ImageUsageFlags.SampledBit;
        }

        if (textureUsage.HasFlag(TextureUsage.Storage))
        {
            imageUsageFlags |= ImageUsageFlags.StorageBit;
        }

        return imageUsageFlags;
    }

    public static SampleCountFlags GetSampleCount(TextureSampleCount sampleCount)
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

    public static DescriptorType GetDescriptorType(ResourceKind kind, ResourceLayoutElementOptions options)
    {
        bool dynamic = options.HasFlag(ResourceLayoutElementOptions.DynamicBinding);

        return kind switch
        {
            ResourceKind.UniformBuffer => dynamic ? DescriptorType.UniformBufferDynamic : DescriptorType.UniformBuffer,
            ResourceKind.StructuredBufferReadOnly or ResourceKind.StructuredBufferReadWrite => dynamic ? DescriptorType.StorageBufferDynamic : DescriptorType.StorageBuffer,
            ResourceKind.TextureReadOnly => DescriptorType.SampledImage,
            ResourceKind.TextureReadWrite => DescriptorType.StorageImage,
            ResourceKind.Sampler => DescriptorType.Sampler,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    public static ShaderStageFlags GetShaderStageFlags(ShaderStages stages)
    {
        ShaderStageFlags shaderStageFlags = ShaderStageFlags.None;

        if (stages.HasFlag(ShaderStages.Vertex))
        {
            shaderStageFlags |= ShaderStageFlags.VertexBit;
        }

        if (stages.HasFlag(ShaderStages.TessellationControl))
        {
            shaderStageFlags |= ShaderStageFlags.TessellationControlBit;
        }

        if (stages.HasFlag(ShaderStages.TessellationEvaluation))
        {
            shaderStageFlags |= ShaderStageFlags.TessellationEvaluationBit;
        }

        if (stages.HasFlag(ShaderStages.Geometry))
        {
            shaderStageFlags |= ShaderStageFlags.GeometryBit;
        }

        if (stages.HasFlag(ShaderStages.Fragment))
        {
            shaderStageFlags |= ShaderStageFlags.FragmentBit;
        }

        if (stages.HasFlag(ShaderStages.Compute))
        {
            shaderStageFlags |= ShaderStageFlags.ComputeBit;
        }

        return shaderStageFlags;
    }

    public static void GetFilter(SamplerFilter filter,
                                 out Filter minFilter,
                                 out Filter magFilter,
                                 out SamplerMipmapMode mipmapMode)
    {
        switch (filter)
        {
            case SamplerFilter.MinPointMagPointMipPoint:
                minFilter = Filter.Nearest;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinPointMagPointMipLinear:
                minFilter = Filter.Nearest;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.MinPointMagLinearMipPoint:
                minFilter = Filter.Nearest;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinPointMagLinearMipLinear:
                minFilter = Filter.Nearest;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.MinLinearMagPointMipPoint:
                minFilter = Filter.Linear;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinLinearMagPointMipLinear:
                minFilter = Filter.Linear;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.MinLinearMagLinearMipPoint:
                minFilter = Filter.Linear;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinLinearMagLinearMipLinear:
                minFilter = Filter.Linear;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.Anisotropic:
                minFilter = Filter.Linear;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter));
        }
    }

    public static SamplerAddressMode GetSamplerAddressMode(AddressMode addressMode)
    {
        return addressMode switch
        {
            AddressMode.Wrap => SamplerAddressMode.Repeat,
            AddressMode.Mirror => SamplerAddressMode.MirroredRepeat,
            AddressMode.Clamp => SamplerAddressMode.ClampToEdge,
            AddressMode.Border => SamplerAddressMode.ClampToBorder,
            _ => throw new ArgumentOutOfRangeException(nameof(addressMode))
        };
    }

    public static CompareOp GetCompareOp(ComparisonKind comparisonKind)
    {
        return comparisonKind switch
        {
            ComparisonKind.Never => CompareOp.Never,
            ComparisonKind.Less => CompareOp.Less,
            ComparisonKind.Equal => CompareOp.Equal,
            ComparisonKind.LessEqual => CompareOp.LessOrEqual,
            ComparisonKind.Greater => CompareOp.Greater,
            ComparisonKind.NotEqual => CompareOp.NotEqual,
            ComparisonKind.GreaterEqual => CompareOp.GreaterOrEqual,
            ComparisonKind.Always => CompareOp.Always,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisonKind))
        };
    }

    public static BorderColor GetBorderColor(SamplerBorderColor borderColor)
    {
        return borderColor switch
        {
            SamplerBorderColor.TransparentBlack => BorderColor.FloatTransparentBlack,
            SamplerBorderColor.OpaqueBlack => BorderColor.FloatOpaqueBlack,
            SamplerBorderColor.OpaqueWhite => BorderColor.FloatOpaqueWhite,
            _ => throw new ArgumentOutOfRangeException(nameof(borderColor))
        };
    }
    #endregion

    #region From Vulkan Format
    public static PixelFormat GetPixelFormat(Format format)
    {
        return format switch
        {
            Format.R8Unorm => PixelFormat.R8UNorm,
            Format.R8SNorm => PixelFormat.R8SNorm,
            Format.R8Uint => PixelFormat.R8UInt,
            Format.R8Sint => PixelFormat.R8SInt,

            Format.R16Unorm => PixelFormat.R16UNorm,
            Format.R16SNorm => PixelFormat.R16SNorm,
            Format.R16Uint => PixelFormat.R16UInt,
            Format.R16Sint => PixelFormat.R16SInt,
            Format.R16Sfloat => PixelFormat.R16Float,

            Format.R32Uint => PixelFormat.R32UInt,
            Format.R32Sint => PixelFormat.R32SInt,
            Format.R32Sfloat => PixelFormat.R32Float,
            Format.D32Sfloat => PixelFormat.R32Float,

            Format.R8G8Unorm => PixelFormat.R8G8UNorm,
            Format.R8G8SNorm => PixelFormat.R8G8SNorm,
            Format.R8G8Uint => PixelFormat.R8G8UInt,
            Format.R8G8Sint => PixelFormat.R8G8SInt,

            Format.R16G16Unorm => PixelFormat.R16G16UNorm,
            Format.R16G16SNorm => PixelFormat.R16G16SNorm,
            Format.R16G16Uint => PixelFormat.R16G16UInt,
            Format.R16G16Sint => PixelFormat.R16G16SInt,
            Format.R16G16Sfloat => PixelFormat.R16G16Float,

            Format.R32G32Uint => PixelFormat.R32G32UInt,
            Format.R32G32Sint => PixelFormat.R32G32SInt,
            Format.R32G32Sfloat => PixelFormat.R32G32Float,

            Format.R8G8B8A8Unorm => PixelFormat.R8G8B8A8UNorm,
            Format.R8G8B8A8Srgb => PixelFormat.R8G8B8A8UNormSRgb,
            Format.B8G8R8A8Unorm => PixelFormat.B8G8R8A8UNorm,
            Format.B8G8R8A8Srgb => PixelFormat.B8G8R8A8UNormSRgb,
            Format.R8G8B8A8SNorm => PixelFormat.R8G8B8A8SNorm,
            Format.R8G8B8A8Uint => PixelFormat.R8G8B8A8UInt,
            Format.R8G8B8A8Sint => PixelFormat.R8G8B8A8SInt,

            Format.R16G16B16A16Unorm => PixelFormat.R16G16B16A16UNorm,
            Format.R16G16B16A16SNorm => PixelFormat.R16G16B16A16SNorm,
            Format.R16G16B16A16Uint => PixelFormat.R16G16B16A16UInt,
            Format.R16G16B16A16Sint => PixelFormat.R16G16B16A16SInt,
            Format.R16G16B16A16Sfloat => PixelFormat.R16G16B16A16Float,

            Format.R32G32B32A32Uint => PixelFormat.R32G32B32A32UInt,
            Format.R32G32B32A32Sint => PixelFormat.R32G32B32A32SInt,
            Format.R32G32B32A32Sfloat => PixelFormat.R32G32B32A32Float,

            Format.BC1RgbUnormBlock => PixelFormat.BC1RgbUNorm,
            Format.BC1RgbSrgbBlock => PixelFormat.BC1RgbUNormSRgb,
            Format.BC1RgbaUnormBlock => PixelFormat.BC1RgbaUNorm,
            Format.BC1RgbaSrgbBlock => PixelFormat.BC1RgbaUNormSRgb,
            Format.BC2UnormBlock => PixelFormat.BC2UNorm,
            Format.BC2SrgbBlock => PixelFormat.BC2UNormSRgb,
            Format.BC3UnormBlock => PixelFormat.BC3UNorm,
            Format.BC3SrgbBlock => PixelFormat.BC3UNormSRgb,
            Format.BC4UnormBlock => PixelFormat.BC4UNorm,
            Format.BC4SNormBlock => PixelFormat.BC4SNorm,
            Format.BC5UnormBlock => PixelFormat.BC5UNorm,
            Format.BC5SNormBlock => PixelFormat.BC5SNorm,
            Format.BC7UnormBlock => PixelFormat.BC7UNorm,
            Format.BC7SrgbBlock => PixelFormat.BC7UNormSRgb,

            Format.Etc2R8G8B8UnormBlock => PixelFormat.ETC2R8G8B8UNorm,
            Format.Etc2R8G8B8A1UnormBlock => PixelFormat.ETC2R8G8B8A1UNorm,
            Format.Etc2R8G8B8A8UnormBlock => PixelFormat.ETC2R8G8B8A8UNorm,

            Format.D32SfloatS8Uint => PixelFormat.D32FloatS8UInt,
            Format.D24UnormS8Uint => PixelFormat.D24UNormS8UInt,

            Format.A2B10G10R10UnormPack32 => PixelFormat.R10G10B10A2UNorm,
            Format.A2B10G10R10UintPack32 => PixelFormat.R10G10B10A2UInt,
            Format.B10G11R11UfloatPack32 => PixelFormat.R11G11B10Float,

            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }
    #endregion
}