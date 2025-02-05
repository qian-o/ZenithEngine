using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ShaderCompiler;

public static class ResourceFactoryExtensions
{
    public static Shader CompileShader(this ResourceFactory factory,
                                       ShaderStages stage,
                                       string source,
                                       string entryPoint,
                                       Func<string, string>? includeHandler = null,
                                       bool optimize = false)
    {
        List<string> arguments = [];

        arguments.Add($"-T {Profile(stage)}");

        if (stage is > ShaderStages.None and < ShaderStages.RayGeneration)
        {
            arguments.Add($"-E {entryPoint}");
        }

        if (optimize)
        {
            arguments.Add("-O3");
        }
        else
        {
            arguments.Add("-Od");
        }

        arguments.Add("-Zpr");

        if (factory.Context.Backend is Backend.Vulkan)
        {
            arguments.Add("-spirv");

            arguments.Add("-fvk-use-dx-layout");

            arguments.Add("-fvk-b-shift");
            arguments.Add("0");
            arguments.Add("all");

            arguments.Add("-fvk-t-shift");
            arguments.Add($"{Utils.CbvCount}");
            arguments.Add("all");

            arguments.Add("-fvk-u-shift");
            arguments.Add($"{Utils.CbvCount + Utils.SrvCount}");
            arguments.Add("all");

            arguments.Add("-fvk-s-shift");
            arguments.Add($"{Utils.CbvCount + Utils.SrvCount + Utils.UavCount}");
            arguments.Add("all");
        }

        byte[] shaderBytes = DxcCompiler.Compile(source, [.. arguments], includeHandler);

        ShaderDesc shaderDesc = ShaderDesc.Default(stage, shaderBytes, entryPoint);

        return factory.CreateShader(in shaderDesc);
    }

    private static string Profile(ShaderStages stage)
    {
        return stage switch
        {
            ShaderStages.Vertex => "vs_6_6",
            ShaderStages.Hull => "hs_6_6",
            ShaderStages.Domain => "ds_6_6",
            ShaderStages.Geometry => "gs_6_6",
            ShaderStages.Pixel => "ps_6_6",
            ShaderStages.Compute => "cs_6_6",
            _ => "lib_6_6"
        };
    }
}
