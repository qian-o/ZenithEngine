using Silk.NET.SPIRV.Reflect;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

internal class SpvFormats
{
    #region To ZenithEngine
    public static ShaderStages GetShaderStages(ShaderStageFlagBits shaderStageFlagBits)
    {
        ShaderStages stages = ShaderStages.None;

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.VertexBit))
        {
            stages |= ShaderStages.Vertex;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.TessellationControlBit))
        {
            stages |= ShaderStages.Hull;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.TessellationEvaluationBit))
        {
            stages |= ShaderStages.Domain;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.GeometryBit))
        {
            stages |= ShaderStages.Geometry;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.FragmentBit))
        {
            stages |= ShaderStages.Pixel;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.ComputeBit))
        {
            stages |= ShaderStages.Compute;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.RaygenBitKhr))
        {
            stages |= ShaderStages.RayGeneration;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.MissBitKhr))
        {
            stages |= ShaderStages.Miss;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.ClosestHitBitKhr))
        {
            stages |= ShaderStages.ClosestHit;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.AnyHitBitKhr))
        {
            stages |= ShaderStages.AnyHit;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.IntersectionBitKhr))
        {
            stages |= ShaderStages.Intersection;
        }

        if (shaderStageFlagBits.HasFlag(ShaderStageFlagBits.CallableBitKhr))
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
            _ => throw new ZenithEngineException(ZenithEngineException.NotSupported(dType))
        };
    }
    #endregion
}
