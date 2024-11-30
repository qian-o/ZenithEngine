using System.Collections.ObjectModel;
using Silk.NET.SPIRV.Reflect;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public unsafe class ReflectResourceLayout
{
    private readonly ReflectResource[] cache = [];

    internal ReflectResourceLayout(ReflectShaderModule* module)
    {
        List<ReflectResource> resources = [];

        ShaderStages stages = SpvFormats.GetShaderStages(module->ShaderStage);

        for (int i = 0; i < module->DescriptorSetCount; i++)
        {
            ReflectDescriptorSet set = module->DescriptorSets[i];

            for (int j = 0; j < set.BindingCount; j++)
            {
                DescriptorBinding* binding = set.Bindings[j];

                ResourceType type = SpvFormats.GetResourceType(binding->DescriptorType,
                                                               binding->ResourceType);

                resources.Add(new(set.Set,
                                  Utils.PtrToStringUTF8((nint)binding->Name),
                                  Utils.GetSlot(type, binding->Binding),
                                  type,
                                  stages,
                                  binding->Count));
            }
        }

        cache = [.. resources];
    }

    public ReflectResource this[string name] => cache.FirstOrDefault(item => item.Name == name);

    public ReadOnlyDictionary<uint, ResourceLayoutDesc> GetDescs()
    {
        Dictionary<uint, ResourceLayoutDesc> descs = [];

        foreach (IGrouping<uint, ReflectResource> space in cache.GroupBy(static item => item.Space))
        {
            descs[space.Key] = ResourceLayoutDesc.Default([.. space.Select(static item => item.Desc)]);
        }

        return new ReadOnlyDictionary<uint, ResourceLayoutDesc>(descs);
    }
}
