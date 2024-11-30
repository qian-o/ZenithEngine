using Silk.NET.SPIRV.Reflect;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

internal class SpvFormats
{
    #region To ZenithEngine
    public static ShaderStages GetShaderStages(ShaderStageFlagBits stageFlagBits)
    {
        ShaderStages stages = ShaderStages.None;

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.VertexBit))
        {
            stages |= ShaderStages.Vertex;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.TessellationControlBit))
        {
            stages |= ShaderStages.Hull;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.TessellationEvaluationBit))
        {
            stages |= ShaderStages.Domain;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.GeometryBit))
        {
            stages |= ShaderStages.Geometry;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.FragmentBit))
        {
            stages |= ShaderStages.Pixel;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.ComputeBit))
        {
            stages |= ShaderStages.Compute;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.RaygenBitKhr))
        {
            stages |= ShaderStages.RayGeneration;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.MissBitKhr))
        {
            stages |= ShaderStages.Miss;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.ClosestHitBitKhr))
        {
            stages |= ShaderStages.ClosestHit;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.AnyHitBitKhr))
        {
            stages |= ShaderStages.AnyHit;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.IntersectionBitKhr))
        {
            stages |= ShaderStages.Intersection;
        }

        if (stageFlagBits.HasFlag(ShaderStageFlagBits.CallableBitKhr))
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
