using Silk.NET.SPIRV.Reflect;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

internal class SpvFormats
{
    #region To ZenithEngine
    public static ShaderStages GetShaderStages(ShaderStageFlagBits bits)
    {
        ShaderStages stages = ShaderStages.None;

        if (bits.HasFlag(ShaderStageFlagBits.VertexBit))
        {
            stages |= ShaderStages.Vertex;
        }

        if (bits.HasFlag(ShaderStageFlagBits.TessellationControlBit))
        {
            stages |= ShaderStages.Hull;
        }

        if (bits.HasFlag(ShaderStageFlagBits.TessellationEvaluationBit))
        {
            stages |= ShaderStages.Domain;
        }

        if (bits.HasFlag(ShaderStageFlagBits.GeometryBit))
        {
            stages |= ShaderStages.Geometry;
        }

        if (bits.HasFlag(ShaderStageFlagBits.FragmentBit))
        {
            stages |= ShaderStages.Pixel;
        }

        if (bits.HasFlag(ShaderStageFlagBits.ComputeBit))
        {
            stages |= ShaderStages.Compute;
        }

        if (bits.HasFlag(ShaderStageFlagBits.RaygenBitKhr))
        {
            stages |= ShaderStages.RayGeneration;
        }

        if (bits.HasFlag(ShaderStageFlagBits.MissBitKhr))
        {
            stages |= ShaderStages.Miss;
        }

        if (bits.HasFlag(ShaderStageFlagBits.ClosestHitBitKhr))
        {
            stages |= ShaderStages.ClosestHit;
        }

        if (bits.HasFlag(ShaderStageFlagBits.AnyHitBitKhr))
        {
            stages |= ShaderStages.AnyHit;
        }

        if (bits.HasFlag(ShaderStageFlagBits.IntersectionBitKhr))
        {
            stages |= ShaderStages.Intersection;
        }

        if (bits.HasFlag(ShaderStageFlagBits.CallableBitKhr))
        {
            stages |= ShaderStages.Callable;
        }

        return stages;
    }

    public static ResourceType GetResourceType(DescriptorType dType, SpvResourceType rType)
    {
        return dType switch
        {
            DescriptorType.Sampler => ResourceType.Sampler,
            DescriptorType.SampledImage => ResourceType.Texture,
            DescriptorType.StorageImage => ResourceType.TextureReadWrite,
            DescriptorType.UniformBuffer => ResourceType.ConstantBuffer,
            DescriptorType.StorageBuffer => rType is SpvResourceType.Srv ? ResourceType.StructuredBuffer : ResourceType.StructuredBufferReadWrite,
            DescriptorType.AccelerationStructureKhr => ResourceType.AccelerationStructure,
            _ => throw new ArgumentOutOfRangeException(nameof(dType))
        };
    }
    #endregion
}
