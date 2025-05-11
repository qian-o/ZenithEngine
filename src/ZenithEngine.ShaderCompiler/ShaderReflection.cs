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

public class ShaderReflection : IReadOnlyDictionary<string, ShaderBinding>
{
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
                ResourceElementDesc desc = new(stage,
                                               GetResourceType(binding.Kind, parameter.Type),
                                               binding.Index,
                                               GetCount(binding, parameter.Type));

                bindings.Add(namedTypeBinding.Name, new(binding.Space, desc));
            }
        }

        uint space = 0;
        foreach (uint item in bindings.Values.Select(static item => item.Space).Distinct())
        {
            if (item != space)
            {
                throw new ZenithEngineException("The space of the resource is not continuous.");
            }

            space++;
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
                if (!bindings.TryGetValue(binding.Key, out ShaderBinding value))
                {
                    bindings.Add(binding.Key, binding.Value);
                }
                else
                {
                    if (value.Space != binding.Value.Space)
                    {
                        throw new ZenithEngineException("This shader reflection has different spaces.");
                    }

                    ResourceElementDesc desc = value.Desc;

                    desc.Stages |= binding.Value.Desc.Stages;

                    bindings[binding.Key] = new(value.Space, desc);
                }
            }
        }

        cache = new(bindings);
    }

    public ShaderBinding this[string key] => cache[key];

    public IEnumerable<string> Keys => cache.Keys;

    public IEnumerable<ShaderBinding> Values => cache.Values;

    public int Count => cache.Count;

    public bool ContainsKey(string key)
    {
        return cache.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<string, ShaderBinding>> GetEnumerator()
    {
        return cache.GetEnumerator();
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out ShaderBinding value)
    {
        return cache.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ResourceLayoutDesc[] ToResourceLayoutDescs()
    {
        uint[] spaces = [.. cache.Values.Select(static item => item.Space).Distinct()];

        ResourceLayoutDesc[] resourceLayoutDescs = new ResourceLayoutDesc[spaces.Length];

        for (int i = 0; i < spaces.Length; i++)
        {
            uint space = spaces[i];

            ResourceElementDesc[] elements = [.. cache.Values.Where(item => item.Space == space).Select(static item => item.Desc)];

            resourceLayoutDescs[i] = new(elements);
        }

        return resourceLayoutDescs;
    }

    public static ShaderReflection Merge(params ShaderReflection[] reflections)
    {
        return new(reflections);
    }

    private static void ParseParameterBlock(string name,
                                            ShaderStages stage,
                                            Dictionary<string, ShaderBinding> bindings,
                                            SlangType type)
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
                ResourceElementDesc desc = new(stage,
                                               GetResourceType(var.Binding!.Kind, var.Type),
                                               var.Binding.Index,
                                               GetCount(var.Binding, var.Type));

                bindings.Add(varName, new(var.Binding.Space, desc));
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

    private static uint GetCount(SlangBinding binding, SlangType type)
    {
        return type.Kind switch
        {
            SlangTypeKind.Array => type.Array!.ElementCount,
            _ => binding.Count
        };
    }
}
