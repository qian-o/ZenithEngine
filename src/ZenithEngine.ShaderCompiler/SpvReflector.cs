using Silk.NET.SPIRV.Reflect;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler;

public static unsafe class SpvReflector
{
    private static readonly SpvReflect reflect = SpvReflect.GetApi();

    public static ReflectResult Reflect(byte[] shader)
    {
        ReflectShaderModule* module = stackalloc ReflectShaderModule[1];
        reflect.CreateShaderModule((uint)shader.Length, ref shader[0], module);

        ShaderStages stages = SpvFormats.GetShaderStages(module->ShaderStage);

        List<ReflectResource> resources = [];

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

        reflect.DestroyShaderModule(module);

        return new([.. resources]);
    }
}
