using Graphics.Core;
using Graphics.Core.Helpers;
using SharpGen.Runtime;
using Vortice.Dxc;

namespace Graphics.Vulkan.Helpers;

public static unsafe class DxcHelpers
{
    private sealed class IncludeHandler(Func<string, byte[]>? includeResolver) : CallbackBase, IDxcIncludeHandler
    {
        private readonly Dictionary<string, IDxcBlob> cache = [];

        public Result LoadSource(string filename, out IDxcBlob includeSource)
        {
            if (!cache.TryGetValue(filename, out IDxcBlob? blob))
            {
                byte[] includeBytes = includeResolver?.Invoke(filename) ?? [];

                blob = DxcCompiler.Utils.CreateBlob((nint)includeBytes.AsPointer(),
                                                    (uint)includeBytes.Length,
                                                    Dxc.DXC_CP_UTF8);
            }

            includeSource = blob;

            return Result.Ok;
        }

        protected override void DisposeCore(bool disposing)
        {
            foreach (IDxcBlob blob in cache.Values)
            {
                blob.Dispose();
            }

            cache.Clear();
        }
    }

    public static byte[] Compile(ShaderStages stage,
                                 string hlsl,
                                 string entryPoint,
                                 Func<string, byte[]>? includeResolver = null)
    {
        using IncludeHandler includeHandler = new(includeResolver);

        using IDxcResult result = DxcCompiler.Compile(hlsl,
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
        DxcShaderStage dxcShaderStage = stage switch
        {
            ShaderStages.Vertex => DxcShaderStage.Vertex,
            ShaderStages.TessellationControl => DxcShaderStage.Hull,
            ShaderStages.TessellationEvaluation => DxcShaderStage.Domain,
            ShaderStages.Geometry => DxcShaderStage.Geometry,
            ShaderStages.Fragment => DxcShaderStage.Pixel,
            ShaderStages.Compute => DxcShaderStage.Compute,
            ShaderStages.RayGeneration or
            ShaderStages.Miss or
            ShaderStages.ClosestHit or
            ShaderStages.AnyHit or
            ShaderStages.Intersection or
            ShaderStages.Callable or
            ShaderStages.Library => DxcShaderStage.Library,
            _ => throw new NotSupportedException("Unsupported shader stage.")
        };

        return DxcCompiler.GetShaderProfile(dxcShaderStage, DxcShaderModel.Model6_4);
    }
}
