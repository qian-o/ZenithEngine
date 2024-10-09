using Graphics.Core;
using Graphics.Core.Helpers;
using SharpGen.Runtime;
using Vortice.Dxc;

namespace Graphics.Vulkan.Helpers;

public static unsafe class DxcHelpers
{
    private sealed class IncludeHandler(Func<string, byte[]>? includeResolver) : CallbackBase, IDxcIncludeHandler
    {
        public Result LoadSource(string filename, out IDxcBlob includeSource)
        {
            byte[] includeBytes = includeResolver?.Invoke(filename) ?? [];

            includeSource = DxcCompiler.Utils.CreateBlob((nint)includeBytes.AsPointer(), includeBytes.Length, Dxc.DXC_CP_UTF8);

            return Result.Ok;
        }
    }

    public static byte[] Compile(ShaderStages stage, string hlsl, string entryPoint, Func<string, byte[]>? includeResolver = null)
    {
        using IncludeHandler includeHandler = new(includeResolver);

        IDxcResult result = DxcCompiler.Compile(hlsl,
                                                GetArguments(stage, entryPoint),
                                                includeHandler);

        if (result.GetStatus() != Result.Ok)
        {
            throw new ShaderCompilationException(result.GetErrors());
        }

        return result.GetResult().AsBytes();
    }

    private static string[] GetArguments(ShaderStages stage, string entryPoint)
    {
        string shaderProfile = GetProfile(stage);
        bool isLib = shaderProfile.Contains("lib");

        return
        [
            "-fvk-use-scalar-layout",
            "-spirv",
            "-T", shaderProfile,
             isLib ? string.Empty : "-E", entryPoint,
            $"-fspv-target-env=vulkan{Context.ApiVersion.Major}.{Context.ApiVersion.Minor}"
         ];
    }

    private static string GetProfile(ShaderStages stage)
    {
        return stage switch
        {
            ShaderStages.Vertex => "vs_6_3",
            ShaderStages.TessellationControl => "hs_6_3",
            ShaderStages.TessellationEvaluation => "ds_6_3",
            ShaderStages.Geometry => "gs_6_3",
            ShaderStages.Fragment => "ps_6_3",
            ShaderStages.Compute => "cs_6_3",
            ShaderStages.RayGeneration or
            ShaderStages.AnyHit or
            ShaderStages.ClosestHit or
            ShaderStages.Miss or
            ShaderStages.Intersection or
            ShaderStages.Callable or
            ShaderStages.Library => "lib_6_3",
            _ => throw new NotSupportedException("Unsupported shader stage.")
        };
    }
}
