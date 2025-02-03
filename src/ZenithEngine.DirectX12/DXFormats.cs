using Silk.NET.Core.Native;
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
            TextureType.Texture1D or
            TextureType.Texture1DArray => ResourceDimension.Texture1D,

            TextureType.Texture2D or
            TextureType.Texture2DArray or
            TextureType.TextureCube or
            TextureType.TextureCubeArray => ResourceDimension.Texture2D,

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
            TextureSampleCount.Count1 => new(1, 0),
            TextureSampleCount.Count2 => new(2, 0),
            TextureSampleCount.Count4 => new(4, 0),
            TextureSampleCount.Count8 => new(8, 0),
            TextureSampleCount.Count16 => new(16, 0),
            TextureSampleCount.Count32 => new(32, 0),
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(count))
        };
    }

    public static Filter GetFilter(SamplerFilter filter, bool isComparison)
    {
        return filter switch
        {
            SamplerFilter.MinPointMagPointMipPoint => isComparison ? Filter.ComparisonMinMagMipPoint : Filter.MinMagMipPoint,
            SamplerFilter.MinPointMagPointMipLinear => isComparison ? Filter.ComparisonMinMagPointMipLinear : Filter.MinMagPointMipLinear,
            SamplerFilter.MinPointMagLinearMipPoint => isComparison ? Filter.ComparisonMinPointMagLinearMipPoint : Filter.MinPointMagLinearMipPoint,
            SamplerFilter.MinPointMagLinearMipLinear => isComparison ? Filter.ComparisonMinPointMagMipLinear : Filter.MinPointMagMipLinear,
            SamplerFilter.MinLinearMagPointMipPoint => isComparison ? Filter.ComparisonMinLinearMagMipPoint : Filter.MinLinearMagMipPoint,
            SamplerFilter.MinLinearMagPointMipLinear => isComparison ? Filter.ComparisonMinLinearMagPointMipLinear : Filter.MinLinearMagPointMipLinear,
            SamplerFilter.MinLinearMagLinearMipPoint => isComparison ? Filter.ComparisonMinMagLinearMipPoint : Filter.MinMagLinearMipPoint,
            SamplerFilter.MinLinearMagLinearMipLinear => isComparison ? Filter.ComparisonMinMagMipLinear : Filter.MinMagMipLinear,
            SamplerFilter.Anisotropic => isComparison ? Filter.ComparisonAnisotropic : Filter.Anisotropic,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(filter))
        };
    }

    public static TextureAddressMode GetTextureAddressMode(AddressMode mode)
    {
        return mode switch
        {
            AddressMode.Wrap => TextureAddressMode.Wrap,
            AddressMode.Mirror => TextureAddressMode.Mirror,
            AddressMode.Clamp => TextureAddressMode.Clamp,
            AddressMode.Border => TextureAddressMode.Border,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(mode))
        };
    }

    public static ComparisonFunc GetComparisonFunc(ComparisonFunction func)
    {
        return func switch
        {
            ComparisonFunction.Never => ComparisonFunc.Never,
            ComparisonFunction.Less => ComparisonFunc.Less,
            ComparisonFunction.Equal => ComparisonFunc.Equal,
            ComparisonFunction.LessEqual => ComparisonFunc.LessEqual,
            ComparisonFunction.Greater => ComparisonFunc.Greater,
            ComparisonFunction.NotEqual => ComparisonFunc.NotEqual,
            ComparisonFunction.GreaterEqual => ComparisonFunc.GreaterEqual,
            ComparisonFunction.Always => ComparisonFunc.Always,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(func))
        };
    }

    public static DxFillMode GetFillMode(FillMode mode)
    {
        return mode switch
        {
            FillMode.Solid => DxFillMode.Solid,
            FillMode.Wireframe => DxFillMode.Wireframe,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(mode))
        };
    }

    public static DxCullMode GetCullMode(CullMode mode)
    {
        return mode switch
        {
            CullMode.None => DxCullMode.None,
            CullMode.Back => DxCullMode.Back,
            CullMode.Front => DxCullMode.Front,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(mode))
        };
    }

    public static StencilOp GetStencilOp(StencilOperation operation)
    {
        return operation switch
        {
            StencilOperation.Keep => StencilOp.Keep,
            StencilOperation.Zero => StencilOp.Zero,
            StencilOperation.Replace => StencilOp.Replace,
            StencilOperation.IncrementAndClamp => StencilOp.IncrSat,
            StencilOperation.DecrementAndClamp => StencilOp.DecrSat,
            StencilOperation.Invert => StencilOp.Invert,
            StencilOperation.IncrementAndWrap => StencilOp.Incr,
            StencilOperation.DecrementAndWrap => StencilOp.Decr,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(operation))
        };
    }

    public static DxBlend GetBlend(Blend blend)
    {
        return blend switch
        {
            Blend.Zero => DxBlend.Zero,
            Blend.One => DxBlend.One,
            Blend.SourceAlpha => DxBlend.SrcAlpha,
            Blend.InverseSourceAlpha => DxBlend.InvSrcAlpha,
            Blend.DestinationAlpha => DxBlend.DestAlpha,
            Blend.InverseDestinationAlpha => DxBlend.InvDestAlpha,
            Blend.SourceColor => DxBlend.SrcColor,
            Blend.InverseSourceColor => DxBlend.InvSrcColor,
            Blend.DestinationColor => DxBlend.DestColor,
            Blend.InverseDestinationColor => DxBlend.InvDestColor,
            Blend.BlendFactor => DxBlend.BlendFactor,
            Blend.InverseBlendFactor => DxBlend.InvBlendFactor,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(blend))
        };
    }

    public static BlendOp GetBlendOp(BlendOperation operation)
    {
        return operation switch
        {
            BlendOperation.Add => BlendOp.Add,
            BlendOperation.Subtract => BlendOp.Subtract,
            BlendOperation.ReverseSubtract => BlendOp.RevSubtract,
            BlendOperation.Min => BlendOp.Min,
            BlendOperation.Max => BlendOp.Max,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(operation))
        };
    }

    public static ColorWriteEnable GetColorWriteEnable(ColorWriteChannels channels)
    {
        return channels switch
        {
            ColorWriteChannels.None => ColorWriteEnable.None,
            ColorWriteChannels.Red => ColorWriteEnable.Red,
            ColorWriteChannels.Green => ColorWriteEnable.Green,
            ColorWriteChannels.Blue => ColorWriteEnable.Blue,
            ColorWriteChannels.Alpha => ColorWriteEnable.Alpha,
            ColorWriteChannels.All => ColorWriteEnable.All,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(channels))
        };
    }

    public static Format GetFormat(ElementFormat format)
    {
        return format switch
        {
            ElementFormat.UByte1 => Format.FormatR8Uint,
            ElementFormat.UByte2 => Format.FormatR8G8Uint,
            ElementFormat.UByte4 => Format.FormatR8G8B8A8Uint,

            ElementFormat.Byte1 => Format.FormatR8Sint,
            ElementFormat.Byte2 => Format.FormatR8G8Sint,
            ElementFormat.Byte4 => Format.FormatR8G8B8A8Sint,

            ElementFormat.UByte1Normalized => Format.FormatR8Unorm,
            ElementFormat.UByte2Normalized => Format.FormatR8G8Unorm,
            ElementFormat.UByte4Normalized => Format.FormatR8G8B8A8Unorm,

            ElementFormat.Byte1Normalized => Format.FormatR8SNorm,
            ElementFormat.Byte2Normalized => Format.FormatR8G8SNorm,
            ElementFormat.Byte4Normalized => Format.FormatR8G8B8A8SNorm,

            ElementFormat.UShort1 => Format.FormatR16Uint,
            ElementFormat.UShort2 => Format.FormatR16G16Uint,
            ElementFormat.UShort4 => Format.FormatR16G16B16A16Uint,

            ElementFormat.Short1 => Format.FormatR16Sint,
            ElementFormat.Short2 => Format.FormatR16G16Sint,
            ElementFormat.Short4 => Format.FormatR16G16B16A16Sint,

            ElementFormat.UShort1Normalized => Format.FormatR16Unorm,
            ElementFormat.UShort2Normalized => Format.FormatR16G16Unorm,
            ElementFormat.UShort4Normalized => Format.FormatR16G16B16A16Unorm,

            ElementFormat.Short1Normalized => Format.FormatR16SNorm,
            ElementFormat.Short2Normalized => Format.FormatR16G16SNorm,
            ElementFormat.Short4Normalized => Format.FormatR16G16B16A16SNorm,

            ElementFormat.Half1 => Format.FormatR16Float,
            ElementFormat.Half2 => Format.FormatR16G16Float,
            ElementFormat.Half4 => Format.FormatR16G16B16A16Float,

            ElementFormat.Float1 => Format.FormatR32Float,
            ElementFormat.Float2 => Format.FormatR32G32Float,
            ElementFormat.Float3 => Format.FormatR32G32B32Float,
            ElementFormat.Float4 => Format.FormatR32G32B32A32Float,

            ElementFormat.UInt1 => Format.FormatR32Uint,
            ElementFormat.UInt2 => Format.FormatR32G32Uint,
            ElementFormat.UInt3 => Format.FormatR32G32B32Uint,
            ElementFormat.UInt4 => Format.FormatR32G32B32A32Uint,

            ElementFormat.Int1 => Format.FormatR32Sint,
            ElementFormat.Int2 => Format.FormatR32G32Sint,
            ElementFormat.Int3 => Format.FormatR32G32B32Sint,
            ElementFormat.Int4 => Format.FormatR32G32B32A32Sint,

            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(format))
        };
    }

    public static InputClassification GetInputClassification(VertexStepFunction function)
    {
        return function switch
        {
            VertexStepFunction.PerVertexData => InputClassification.PerVertexData,
            VertexStepFunction.PerInstanceData => InputClassification.PerInstanceData,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(function))
        };
    }

    public static ShaderVisibility GetShaderVisibility(ShaderStages stage)
    {
        return stage switch
        {
            ShaderStages.Vertex => ShaderVisibility.Vertex,
            ShaderStages.Hull => ShaderVisibility.Hull,
            ShaderStages.Domain => ShaderVisibility.Domain,
            ShaderStages.Geometry => ShaderVisibility.Geometry,
            ShaderStages.Pixel => ShaderVisibility.Pixel,
            _ => ShaderVisibility.All
        };
    }

    public static PrimitiveTopologyType GetPrimitiveTopologyType(PrimitiveTopology topology)
    {
        return topology switch
        {
            PrimitiveTopology.PointList => PrimitiveTopologyType.Point,

            PrimitiveTopology.LineList or
            PrimitiveTopology.LineStrip or
            PrimitiveTopology.LineListWithAdjacency or
            PrimitiveTopology.LineStripWithAdjacency => PrimitiveTopologyType.Line,

            PrimitiveTopology.TriangleList or
            PrimitiveTopology.TriangleStrip or
            PrimitiveTopology.TriangleListWithAdjacency or
            PrimitiveTopology.TriangleStripWithAdjacency => PrimitiveTopologyType.Triangle,

            >= PrimitiveTopology.PatchList => PrimitiveTopologyType.Patch,

            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(topology))
        };
    }

    public static D3DPrimitiveTopology GetPrimitiveTopology(PrimitiveTopology topology)
    {
        return topology switch
        {
            PrimitiveTopology.PointList => D3DPrimitiveTopology.D3DPrimitiveTopologyPointlist,
            PrimitiveTopology.LineList => D3DPrimitiveTopology.D3DPrimitiveTopologyLinelist,
            PrimitiveTopology.LineStrip => D3DPrimitiveTopology.D3DPrimitiveTopologyLinestrip,
            PrimitiveTopology.TriangleList => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist,
            PrimitiveTopology.TriangleStrip => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestrip,
            PrimitiveTopology.LineListWithAdjacency => D3DPrimitiveTopology.D3DPrimitiveTopologyLinelistAdj,
            PrimitiveTopology.LineStripWithAdjacency => D3DPrimitiveTopology.D3DPrimitiveTopologyLinestripAdj,
            PrimitiveTopology.TriangleListWithAdjacency => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelistAdj,
            PrimitiveTopology.TriangleStripWithAdjacency => D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestripAdj,
            >= PrimitiveTopology.PatchList => D3DPrimitiveTopology.D3DPrimitiveTopology1ControlPointPatchlist + (PrimitiveTopology.PatchList - topology),
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(topology))
        };
    }

    public static CommandListType GetCommandListType(CommandProcessorType type)
    {
        return type switch
        {
            CommandProcessorType.Graphics => CommandListType.Direct,
            CommandProcessorType.Compute => CommandListType.Compute,
            CommandProcessorType.Copy => CommandListType.Copy,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type))
        };
    }

    public static Format GetFormat(IndexFormat format)
    {
        return format switch
        {
            IndexFormat.UInt16 => Format.FormatR16Uint,
            IndexFormat.UInt32 => Format.FormatR32Uint,
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(format))
        };
    }
    #endregion
}
