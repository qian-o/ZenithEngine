using System.Collections.ObjectModel;
using Slangc.NET;
using Slangc.NET.Enums;
using Slangc.NET.Models;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public class ShaderReflection
{
    private record ShaderBinding(string Name, ShaderStages Stages, ResourceType Type, uint Space, uint Index, uint Count);

    private readonly ReadOnlyDictionary<string, ShaderBinding> cache;

    internal ShaderReflection(ShaderStages stage, SlangReflection slangReflection)
    {
        SlangEntryPoint entryPoint = slangReflection.EntryPoints[0];

        Dictionary<string, ShaderBinding> bindings = [];

        foreach (SlangNamedTypeBinding namedTypeBinding in entryPoint.Bindings)
        {
            SlangBinding binding = namedTypeBinding.Binding;

            if (!binding.Used)
            {
                continue;
            }

            SlangParameter parameter = slangReflection.Parameters.First(item => item.Name == namedTypeBinding.Name);

            if (binding.Kind is SlangParameterCategory.SubElementRegisterSpace)
            {
                if (parameter.Type.Kind is not SlangTypeKind.ParameterBlock)
                {
                    throw new ZenithEngineException(ExceptionHelpers.NotSupported(parameter.Type.Kind));
                }

                ParseParameterBlock(namedTypeBinding.Name, stage, bindings, parameter.Type);
            }
            else
            {
                bindings.Add(namedTypeBinding.Name, new(namedTypeBinding.Name,
                                                        stage,
                                                        GetResourceType(binding.Kind, parameter.Type),
                                                        binding.Space,
                                                        binding.Index,
                                                        binding.Count));
            }
        }

        cache = new(bindings);
    }

    internal ShaderReflection(ShaderReflection[] reflections)
    {
        Dictionary<string, ShaderBinding> bindings = [];

        foreach (ShaderReflection reflection in reflections)
        {
            foreach (KeyValuePair<string, ShaderBinding> binding in reflection.cache)
            {
                if (!bindings.TryGetValue(binding.Key, out ShaderBinding? value))
                {
                    bindings.Add(binding.Key, binding.Value);
                }
                else
                {
                    ShaderStages stages = value.Stages | binding.Value.Stages;

                    bindings[binding.Key] = new(binding.Value.Name,
                                                stages,
                                                binding.Value.Type,
                                                binding.Value.Space,
                                                binding.Value.Index,
                                                binding.Value.Count);
                }
            }
        }

        cache = new(bindings);
    }

    public ShaderReflection Merge(ShaderReflection other)
    {
        return new ShaderReflection([this, other]);
    }

    private static void ParseParameterBlock(string name, ShaderStages stage, Dictionary<string, ShaderBinding> bindings, SlangType type)
    {
        foreach (SlangVar var in type.ParameterBlock!.ElementType.Struct!.Fields)
        {
            string varName = $"{name}.{var.Name}";

            if (var.Type.Kind is SlangTypeKind.ParameterBlock)
            {
                ParseParameterBlock(varName, stage, bindings, var.Type);
            }
            else
            {
                bindings.Add(varName, new(varName,
                                          stage,
                                          GetResourceType(var.Binding!.Kind, var.Type),
                                          var.Binding.Space,
                                          var.Binding.Index,
                                          var.Binding.Count));
            }
        }
    }

    private static ResourceType GetResourceType(SlangParameterCategory parameterCategory, SlangType type)
    {
        return parameterCategory switch
        {
            SlangParameterCategory.ConstantBuffer or
            SlangParameterCategory.Uniform => ResourceType.ConstantBuffer,

            SlangParameterCategory.SamplerState => ResourceType.Sampler,

            SlangParameterCategory.ShaderResource or
            SlangParameterCategory.UnorderedAccess or
            SlangParameterCategory.DescriptorTableSlot => Indistinct(type),

            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(parameterCategory))
        };

        static ResourceType Indistinct(SlangType type)
        {
            return type.Kind switch
            {
                SlangTypeKind.Array => Indistinct(type.Array!.ElementType),
                SlangTypeKind.ConstantBuffer => ResourceType.ConstantBuffer,
                SlangTypeKind.Resource => Resource(type),
                SlangTypeKind.SamplerState => ResourceType.Sampler,
                _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type.Kind))
            };
        }

        static ResourceType Resource(SlangType type)
        {
            bool isReadWrite = type.Resource!.Access is SlangResourceAccess.ReadWrite;

            return type.Resource!.BaseShape switch
            {
                SlangResourceShape.Texture1D or
                SlangResourceShape.Texture2D or
                SlangResourceShape.Texture3D or
                SlangResourceShape.TextureCube => isReadWrite ? ResourceType.TextureReadWrite : ResourceType.Texture,

                SlangResourceShape.StructuredBuffer => isReadWrite ? ResourceType.StructuredBufferReadWrite : ResourceType.StructuredBuffer,

                SlangResourceShape.AccelerationStructure => ResourceType.AccelerationStructure,

                _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type.Resource.BaseShape))
            };
        }
    }
}
