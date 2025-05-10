using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Slangc.NET;
using Slangc.NET.Enums;
using Slangc.NET.Models;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public class ShaderReflection : IReadOnlyDictionary<string, ResourceElementDesc>
{
    private readonly ReadOnlyDictionary<string, ResourceElementDesc> cache;

    internal ShaderReflection(ShaderStages stage, SlangReflection slangReflection)
    {
        SlangEntryPoint entryPoint = slangReflection.EntryPoints[0];

        Dictionary<string, ResourceElementDesc> keyValues = [];

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

                ParseParameterBlock(namedTypeBinding.Name, stage, keyValues, parameter.Type);
            }
            else
            {
                keyValues.Add(namedTypeBinding.Name, new(stage,
                                                         GetResourceType(binding.Kind, parameter.Type),
                                                         binding.Index,
                                                         binding.Count));
            }
        }

        cache = new(keyValues);
    }

    internal ShaderReflection(ShaderReflection[] reflections)
    {
        Dictionary<string, ResourceElementDesc> keyValues = [];

        foreach (ShaderReflection reflection in reflections)
        {
            foreach (KeyValuePair<string, ResourceElementDesc> binding in reflection.cache)
            {
                if (!keyValues.TryGetValue(binding.Key, out ResourceElementDesc value))
                {
                    keyValues.Add(binding.Key, binding.Value);
                }
                else
                {
                    ShaderStages stages = value.Stages | binding.Value.Stages;

                    keyValues[binding.Key] = new(stages,
                                                 binding.Value.Type,
                                                 binding.Value.Index,
                                                 binding.Value.Count);
                }
            }
        }

        cache = new(keyValues);
    }

    public ResourceElementDesc this[string key] => cache[key];

    public IEnumerable<string> Keys => cache.Keys;

    public IEnumerable<ResourceElementDesc> Values => cache.Values;

    public int Count => cache.Count;

    public bool ContainsKey(string key)
    {
        return cache.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<string, ResourceElementDesc>> GetEnumerator()
    {
        return cache.GetEnumerator();
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out ResourceElementDesc value)
    {
        return cache.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ShaderReflection Merge(ShaderReflection other)
    {
        return new ShaderReflection([this, other]);
    }

    private static void ParseParameterBlock(string name,
                                            ShaderStages stage,
                                            Dictionary<string, ResourceElementDesc> keyValues,
                                            SlangType type)
    {
        foreach (SlangVar var in type.ParameterBlock!.ElementType.Struct!.Fields)
        {
            string varName = $"{name}.{var.Name}";

            if (var.Type.Kind is SlangTypeKind.ParameterBlock)
            {
                ParseParameterBlock(varName, stage, keyValues, var.Type);
            }
            else
            {
                keyValues.Add(varName, new(stage,
                                           GetResourceType(var.Binding!.Kind, var.Type),
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
