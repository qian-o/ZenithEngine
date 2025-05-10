using Slangc.NET;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ShaderCompiler;

public static class ResourceFactoryExtensions
{
    public static Shader CompileShader(this ResourceFactory factory,
                                       string path,
                                       ShaderStages stage,
                                       string entryPoint,
                                       out ShaderReflection reflection)
    {
        List<string> arguments =
        [
            path,
            "-profile", "sm_6_6",
            "-stage", stage.ToString().ToLowerInvariant(),
            "-entry", entryPoint,
            "-O3",
            "-matrix-layout-row-major"
        ];

        switch (factory.Context.Backend)
        {
            case Backend.DirectX12:
                {
                    arguments.AddRange(["-target", "dxil"]);
                }
                break;
            case Backend.Vulkan:
                {
                    arguments.AddRange(["-fvk-use-dx-layout", "-fvk-use-entrypoint-name"]);

                    arguments.AddRange(["-target", "spirv"]);
                }
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(factory.Context.Backend));
        }

        byte[] shaderBytes = SlangCompiler.CompileWithReflection([.. arguments], out SlangReflection slangReflection);

        reflection = new(stage, slangReflection);

        ShaderDesc shaderDesc = new(stage, shaderBytes, entryPoint);

        return factory.CreateShader(in shaderDesc);
    }
}
