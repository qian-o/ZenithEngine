using Slangc.NET;

namespace ZenithEngine.ShaderCompiler;

public class ShaderReflection
{
    internal ShaderReflection(SlangReflection slangReflection)
    {
    }

    internal ShaderReflection(ShaderReflection[] reflections)
    {
    }

    public ShaderReflection Merge(ShaderReflection other)
    {
        return new ShaderReflection([this, other]);
    }
}
