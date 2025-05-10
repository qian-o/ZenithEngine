using Slangc.NET;
using Slangc.NET.Enums;
using Slangc.NET.Models;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public class ShaderReflection
{
    private record ShaderBinding(string Name, ShaderStages Stages, ResourceType type, uint Space, uint Index, uint Count);

    private readonly Dictionary<string, ShaderBinding> bindings = [];

    internal ShaderReflection(ShaderStages stage, SlangReflection slangReflection)
    {
        SlangEntryPoint entryPoint = slangReflection.EntryPoints[0];

        foreach (SlangNamedTypeBinding namedTypeBinding in entryPoint.Bindings)
        {
            SlangBinding binding = namedTypeBinding.Binding;

            //if (!binding.Used)
            //{
            //    continue;
            //}

            if (binding.Kind is SlangParameterCategory.SubElementRegisterSpace)
            {
                // 递归结构体。
            }
            else
            {
                SlangParameter parameter = slangReflection.Parameters.First(item => item.Name == namedTypeBinding.Name);

                bindings.Add(namedTypeBinding.Name, new(namedTypeBinding.Name,
                                                        stage,
                                                        GetResourceType(binding.Kind, parameter.Type),
                                                        binding.Space,
                                                        binding.Index,
                                                        binding.Count));
            }
        }
    }

    internal ShaderReflection(ShaderReflection[] reflections)
    {
    }

    public ShaderReflection Merge(ShaderReflection other)
    {
        return new ShaderReflection([this, other]);
    }

    private static ResourceType GetResourceType(SlangParameterCategory parameterCategory, SlangType type)
    {
        return parameterCategory switch
        {
            SlangParameterCategory.ConstantBuffer or
            SlangParameterCategory.Uniform => ResourceType.ConstantBuffer,

            SlangParameterCategory.SamplerState => ResourceType.Sampler,

            SlangParameterCategory.ShaderResource => ResourceCategory(type, false),

            SlangParameterCategory.UnorderedAccess => ResourceCategory(type, true),

            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(parameterCategory))
        };

        static ResourceType ResourceCategory(SlangType type, bool isReadWrite)
        {
            if (type.Kind is SlangTypeKind.Array)
            {
                return ResourceCategory(type.Array!.ElementType, isReadWrite);
            }

            if (type.Kind is not SlangTypeKind.Resource)
            {
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(type.Kind));
            }

            return type.Resource!.BaseShape switch
            {
                SlangResourceShape.Texture1D or
                SlangResourceShape.Texture2D or
                SlangResourceShape.Texture3D or
                SlangResourceShape.TextureCube => isReadWrite ? ResourceType.TextureReadWrite : ResourceType.Texture,

                SlangResourceShape.StructuredBuffer => isReadWrite ? ResourceType.StructuredBufferReadWrite : ResourceType.StructuredBuffer,

                SlangResourceShape.AccelerationStructure => ResourceType.AccelerationStructure,

                _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type.Resource.BaseShape)),
            };
        }
    }
}
