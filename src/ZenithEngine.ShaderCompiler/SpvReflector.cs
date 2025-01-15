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
        reflect.CreateShaderModule((uint)shader.Length, in shader[0], module);

        ShaderStages stages = SpvFormats.GetShaderStages(module->ShaderStage);

        List<ReflectResource> resources = [];

        for (uint i = 0; i < module->DescriptorSetCount; i++)
        {
            ReflectDescriptorSet set = module->DescriptorSets[(int)i];

            for (uint j = 0; j < set.BindingCount; j++)
            {
                DescriptorBinding* binding = set.Bindings[j];

                ResourceType type = SpvFormats.GetResourceType(binding->DescriptorType, binding->ResourceType);

                resources.Add(new(stages,
                                  type,
                                  GetSlot(type, binding->Binding),
                                  set.Set,
                                  Utils.PtrToStringUTF8((nint)binding->Name),
                                  binding->Count));
            }
        }

        reflect.DestroyShaderModule(module);

        return new([.. resources]);
    }

    private static uint GetSlot(ResourceType type, uint binding)
    {
        return type switch
        {
            ResourceType.ConstantBuffer => binding,

            ResourceType.StructuredBuffer or
            ResourceType.Texture or
            ResourceType.AccelerationStructure => binding - Utils.CbvCount,

            ResourceType.StructuredBufferReadWrite or
            ResourceType.TextureReadWrite => binding - Utils.CbvCount - Utils.SrvCount,

            ResourceType.Sampler => binding - Utils.CbvCount - Utils.SrvCount - Utils.UavCount,

            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(type))
        };
    }
}
