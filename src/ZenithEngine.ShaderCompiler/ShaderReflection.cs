using System.Collections.ObjectModel;
using Slangc.NET;
using Slangc.NET.Enums;
using Slangc.NET.Models;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public class ShaderReflection
{
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
                                               GetType(binding.Kind, parameter.Type),
                                               binding.Index,
                                               GetCount(parameter.Type, binding.Count));

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

        Bindings = new(bindings);
    }

    internal ShaderReflection(ShaderReflection[] reflections)
    {
        Dictionary<string, ShaderBinding> bindings = [];
        foreach (ShaderReflection reflection in reflections)
        {
            foreach (KeyValuePair<string, ShaderBinding> binding in reflection.Bindings)
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

        Bindings = new(bindings);
    }

    public ReadOnlyDictionary<string, ShaderBinding> Bindings { get; }

    public ShaderBinding this[string key] => Bindings[key];

    public ResourceLayoutDesc[] ToResourceLayoutDescs()
    {
        uint[] spaces = [.. Bindings.Values.Select(static item => item.Space).Distinct()];

        ResourceLayoutDesc[] resourceLayoutDescs = new ResourceLayoutDesc[spaces.Length];

        for (int i = 0; i < spaces.Length; i++)
        {
            uint space = spaces[i];

            ResourceElementDesc[] elements = [.. Bindings.Values.Where(item => item.Space == space).Select(static item => item.Desc)];

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
                                               GetType(var.Binding!.Kind, var.Type),
                                               var.Binding.Index,
                                               GetCount(var.Type, var.Binding.Count));

                bindings.Add(varName, new(var.Binding.Space, desc));
            }
        }
    }

    private static ResourceType GetType(SlangParameterCategory kind, SlangType type)
    {
        return kind switch
        {
            SlangParameterCategory.ConstantBuffer or
            SlangParameterCategory.Uniform => ResourceType.ConstantBuffer,

            SlangParameterCategory.SamplerState => ResourceType.Sampler,

            _ => Indistinct(type)
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

    private static uint GetCount(SlangType type, uint defaultCount)
    {
        return type.Kind switch
        {
            SlangTypeKind.Array => type.Array!.ElementCount,
            _ => defaultCount
        };
    }
}