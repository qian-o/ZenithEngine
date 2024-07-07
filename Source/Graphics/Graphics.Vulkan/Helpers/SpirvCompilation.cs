using Graphics.Core;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal unsafe static class SpirvCompilation
{
    private static readonly Shaderc _shaderc;

    static SpirvCompilation()
    {
        _shaderc = Shaderc.GetApi();
    }

    public static byte[] CompileGlslToSpirv(ref readonly ShaderDescription description)
    {
        Compiler* compiler = _shaderc.CompilerInitialize();
        CompileOptions* options = _shaderc.CompileOptionsInitialize();
        CompilationResult* result;

        _shaderc.CompileOptionsSetSourceLanguage(options, SourceLanguage.Glsl);
        _shaderc.CompileOptionsSetTargetEnv(options, TargetEnv.Vulkan, Vk.Version13);

        result = _shaderc.CompileIntoSpv(compiler,
                                         description.ShaderBytes,
                                         (uint)description.ShaderBytes.Length,
                                         GetShadercKind(description.Stage),
                                         string.Empty,
                                         description.EntryPoint,
                                         options);

        if (_shaderc.ResultGetCompilationStatus(result) != CompilationStatus.Success)
        {
            throw new GraphicsException($"Failed to compile shader: {_shaderc.ResultGetErrorMessageS(result)}");
        }

        ReadOnlySpan<byte> spirv = new(_shaderc.ResultGetBytes(result), (int)_shaderc.ResultGetLength(result));

        return spirv.ToArray();
    }

    private static ShaderKind GetShadercKind(ShaderStages stage)
    {
        return stage switch
        {
            ShaderStages.Vertex => ShaderKind.VertexShader,
            ShaderStages.Fragment => ShaderKind.FragmentShader,
            ShaderStages.Compute => ShaderKind.ComputeShader,
            ShaderStages.Geometry => ShaderKind.GeometryShader,
            ShaderStages.TessellationControl => ShaderKind.TessControlShader,
            ShaderStages.TessellationEvaluation => ShaderKind.TessEvaluationShader,
            _ => throw new NotSupportedException()
        };
    }
}
