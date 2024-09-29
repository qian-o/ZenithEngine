using Graphics.Core;
using Silk.NET.Shaderc;

namespace Graphics.Vulkan;

internal static unsafe class ShadercHelpers
{
    private static readonly Shaderc _shaderc;

    static ShadercHelpers()
    {
        _shaderc = Shaderc.GetApi();
    }

    public static byte[] CompileHlslToSpirv(ref readonly ShaderDescription description, Func<string, byte[]>? includeResolver = null)
    {
        using Alloter alloter = new();

        Compiler* compiler = _shaderc.CompilerInitialize();
        CompileOptions* options = _shaderc.CompileOptionsInitialize();
        CompilationResult* result;

        _shaderc.CompileOptionsSetSourceLanguage(options, SourceLanguage.Hlsl);
        _shaderc.CompileOptionsSetTargetEnv(options, TargetEnv.Vulkan, Context.ApiVersion);
        _shaderc.CompileOptionsSetOptimizationLevel(options, OptimizationLevel.Performance);

        _shaderc.CompileOptionsSetIncludeCallbacks(options,
                                                   PfnIncludeResolveFn.From(IncludeCallback),
                                                   PfnIncludeResultReleaseFn.From(IncludeResultReleaseCallback),
                                                   null);

        result = _shaderc.CompileIntoSpv(compiler,
                                         description.ShaderBytes,
                                         (uint)description.ShaderBytes.Length,
                                         GetShadercKind(description.Stage),
                                         string.Empty,
                                         description.EntryPoint,
                                         options);

        if (_shaderc.ResultGetCompilationStatus(result) != CompilationStatus.Success)
        {
            throw new ShaderCompilationException(_shaderc.ResultGetErrorMessageS(result));
        }

        ReadOnlySpan<byte> spirv = new(_shaderc.ResultGetBytes(result), (int)_shaderc.ResultGetLength(result));

        return spirv.ToArray();

        IncludeResult* IncludeCallback(void* userData,
                                       byte* requestedSource,
                                       int type,
                                       byte* requestingSource,
                                       nuint includeDepth)
        {
            string requestedPath = Alloter.GetString(requestedSource);
            string requestedName = Path.GetFileName(requestedPath);

            byte[] includeBytes = includeResolver?.Invoke(requestedPath) ?? [];

            IncludeResult includeResult = new()
            {
                SourceName = alloter.Allocate(requestedName),
                SourceNameLength = (nuint)requestedName.Length,
                Content = alloter.Allocate(includeBytes),
                ContentLength = (nuint)includeBytes.Length
            };

            return alloter.Allocate(includeResult);
        }

        void IncludeResultReleaseCallback(void* userData, IncludeResult* result)
        {
            alloter.Free(result->SourceName);
            alloter.Free(result->Content);
            alloter.Free(result);
        }
    }

    private static ShaderKind GetShadercKind(ShaderStages stage)
    {
        return stage switch
        {
            ShaderStages.Vertex => ShaderKind.VertexShader,
            ShaderStages.TessellationControl => ShaderKind.TessControlShader,
            ShaderStages.TessellationEvaluation => ShaderKind.TessEvaluationShader,
            ShaderStages.Geometry => ShaderKind.GeometryShader,
            ShaderStages.Fragment => ShaderKind.FragmentShader,
            ShaderStages.Compute => ShaderKind.ComputeShader,
            _ => throw new NotSupportedException("Shaderc does not support HLSL RayTracing Shader, please try using DXC compiler.")
        };
    }
}
