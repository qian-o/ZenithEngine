using Silk.NET.SPIRV.Reflect;

namespace ZenithEngine.ShaderCompiler;

public static unsafe class SpvReflector
{
    private static readonly SpvReflect reflect = SpvReflect.GetApi();

    public static ReflectResourceLayout Reflect(byte[] shader)
    {
        ReflectShaderModule* module = stackalloc ReflectShaderModule[1];
        reflect.CreateShaderModule((uint)shader.Length, ref shader[0], module);

        ReflectResourceLayout layout = new(module);

        reflect.DestroyShaderModule(module);

        return layout;
    }
}
