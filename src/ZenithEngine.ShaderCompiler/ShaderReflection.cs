﻿using System.Collections.ObjectModel;
using Slangc.NET;
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
                ParseParameterBlock(bindings, namedTypeBinding.Name, stage, parameter.Type);
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

        uint expectedSpace = 0;
        foreach (uint space in bindings.Values.Select(static item => item.Space).Distinct())
        {
            if (space != expectedSpace)
            {
                throw new ZenithEngineException("The space of the resource is not continuous.");
            }

            expectedSpace++;
        }

        Bindings = new(bindings);
        SpaceCount = expectedSpace;
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
        SpaceCount = (uint)Bindings.Values.Select(static item => item.Space).Distinct().Count();
    }

    public ReadOnlyDictionary<string, ShaderBinding> Bindings { get; }

    public uint SpaceCount { get; }

    public ShaderBinding this[string key] => Bindings[key];

    public ReadOnlyDictionary<string, ShaderBinding> GetBindingsBySpace(uint space)
    {
        Dictionary<string, ShaderBinding> bindings = [];

        foreach (KeyValuePair<string, ShaderBinding> binding in Bindings)
        {
            if (binding.Value.Space == space)
            {
                bindings.Add(binding.Key, binding.Value);
            }
        }

        return new(bindings);
    }

    public static ShaderReflection Merge(params ShaderReflection[] reflections)
    {
        return new(reflections);
    }

    private static void ParseParameterBlock(Dictionary<string, ShaderBinding> bindings,
                                            string name,
                                            ShaderStages stage,
                                            SlangType type)
    {
        foreach (SlangVar var in type.ParameterBlock!.ElementType.Struct!.Fields)
        {
            string varName = $"{name}.{var.Name}";

            if (var.Type.Kind is SlangTypeKind.ParameterBlock)
            {
                ParseParameterBlock(bindings, varName, stage, var.Type);
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

            return type.Resource.BaseShape switch
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