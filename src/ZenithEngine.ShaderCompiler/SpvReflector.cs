using Silk.NET.SPIRV.Reflect;
using ZenithEngine.Common;
using SpvReflect = Silk.NET.SPIRV.Reflect.Reflect;

namespace ZenithEngine.ShaderCompiler;

public static unsafe class SpvReflector
{
    private static readonly SpvReflect reflect = SpvReflect.GetApi();

    public static void Reflect(byte[] shader)
    {
        using MemoryAllocator allocator = new();

        ReflectShaderModule* module = allocator.Alloc<ReflectShaderModule>();
        reflect.CreateShaderModule((uint)shader.Length, ref shader[0], module);

        for (uint i = 0; i < module->DescriptorBindingCount; i++)
        {
            DescriptorBinding binding = module->DescriptorBindings[i];
        }
    }
}
