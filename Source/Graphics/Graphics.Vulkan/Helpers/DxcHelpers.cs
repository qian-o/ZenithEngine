using System.Text;
using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using SharpGen.Runtime;
using Vortice.Dxc;

namespace Graphics.Vulkan.Helpers;

internal static unsafe class DxcHelpers
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

    public static byte[] Compile(ref readonly ShaderDescription description, Func<string, byte[]>? includeResolver = null)
    {
        using IncludeHandler includeHandler = new(includeResolver);

        IDxcResult result = DxcCompiler.Compile(Encoding.UTF8.GetString(description.ShaderBytes),
                                                GetArguments(in description),
                                                includeHandler);

        if (result.GetStatus() != Result.Ok)
        {
            throw new ShaderCompilationException(result.GetErrors());
        }

        return result.GetResult().AsBytes();
    }

    private static string[] GetArguments(ref readonly ShaderDescription shaderDescription)
    {
        return
        [
            "-spirv",
            "-T", GetProfile(in shaderDescription),
            "-E", shaderDescription.EntryPoint,
            $"-fspv-target-env=vulkan{Context.ApiVersion.Major}.{Context.ApiVersion.Minor}",
            "-fvk-use-scalar-layout"
         ];
    }

    private static string GetProfile(ref readonly ShaderDescription shaderDescription)
    {
        return shaderDescription.Stage switch
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
            ShaderStages.Callable => "lib_6_3",
            _ => throw new NotSupportedException("Unsupported shader stage.")
        };
    }
}
