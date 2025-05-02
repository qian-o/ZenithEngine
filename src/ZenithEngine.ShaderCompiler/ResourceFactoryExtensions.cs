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
                                       string entryPoint)
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
                    arguments.AddRange(["-fvk-use-entrypoint-name", "-fvk-use-dx-layout"]);

                    arguments.AddRange(["-fvk-b-shift", "0", "all"]);
                    arguments.AddRange(["-fvk-t-shift", $"{Utils.CbvCount}", "all"]);
                    arguments.AddRange(["-fvk-u-shift", $"{Utils.CbvCount + Utils.SrvCount}", "all"]);
                    arguments.AddRange(["-fvk-s-shift", $"{Utils.CbvCount + Utils.SrvCount + Utils.UavCount}", "all"]);

                    arguments.AddRange(["-target", "spirv"]);
                }
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(factory.Context.Backend));
        }

        byte[] shaderBytes = SlangCompiler.Compile([.. arguments]);

        ShaderDesc shaderDesc = new(stage, shaderBytes, entryPoint);

        return factory.CreateShader(in shaderDesc);
    }
}
