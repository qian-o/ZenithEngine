using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace ZenithEngine.ShaderCompiler;

public static unsafe class DxcCompiler
{
    private static readonly Guid CLSID_DxcUtils = new("6245D6AF-66E0-48FD-80B4-4D271796748C");
    private static readonly Guid CLSID_DxcCompiler = new("73E22D93-E6CE-47F3-B5BF-F0664F39C1B0");

    public static readonly DXC Dxc;
    public static readonly ComPtr<IDxcUtils> DxcUtils;
    public static readonly ComPtr<IDxcCompiler3> DxcCompiler3;

    static DxcCompiler()
    {
        Dxc = DXC.GetApi();
        Dxc.CreateInstance(ref CLSID_DxcUtils, out DxcUtils);
        Dxc.CreateInstance(ref CLSID_DxcCompiler, out DxcCompiler3);
    }

    public static byte[] Compile(ShaderStages stage,
                                 string source,
                                 string entryPoint,
                                 Func<string, string>? includeHandler = null)
    {
        using IncludeHandler handler = new(includeHandler);

        using MemoryAllocator allocator = new();

        using ComPtr<IDxcResult> result = default;
        using ComPtr<IDxcBlobUtf8> eb = default;
        using ComPtr<IDxcBlob> rb = default;

        string[] arguments = GetArguments(stage, entryPoint);

        DxcBuffer buffer = new()
        {
            Ptr = allocator.AllocUTF8(source),
            Size = Utils.CalcSizeStringUTF8(source),
            Encoding = DXC.CPUtf8
        };

        DxcCompiler3.Compile(in buffer,
                             (char**)allocator.AllocUni(arguments),
                             (uint)arguments.Length,
                             (IDxcIncludeHandler*)handler.Handle,
                             SilkMarshal.GuidPtrOf<IDxcResult>(),
                             (void**)result.GetAddressOf());

        int status;
        result.GetStatus(&status);

        if (status is not 0)
        {
            result.GetErrorBuffer((IDxcBlobEncoding**)eb.GetAddressOf());

            throw new ZenithEngineException($"Shader compilation failed: {Utils.PtrToStringUTF8((nint)eb.GetBufferPointer())}");
        }

        result.GetResult(rb.GetAddressOf());

        return [.. new ReadOnlySpan<byte>(rb.GetBufferPointer(), (int)rb.GetBufferSize())];
    }

    private static string[] GetArguments(ShaderStages stage, string entryPoint)
    {
        List<string> arguments = [];

        arguments.Add($"-T {GetProfile(stage)}");

        if (stage is > ShaderStages.None and < ShaderStages.RayGeneration)
        {
            arguments.Add($"-E {entryPoint}");
        }

        arguments.Add("-Zpr");

        arguments.Add("-fvk-use-dx-layout");

        arguments.Add("-fvk-b-shift");
        arguments.Add($"0");
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

        arguments.Add("-spirv");
        arguments.Add("-fspv-target-env=vulkan1.3");

        return [.. arguments];
    }

    private static string GetProfile(ShaderStages stage)
    {
        return stage switch
        {
            ShaderStages.Vertex => "vs_6_8",
            ShaderStages.Hull => "hs_6_8",
            ShaderStages.Domain => "ds_6_8",
            ShaderStages.Geometry => "gs_6_8",
            ShaderStages.Pixel => "ps_6_8",
            ShaderStages.Compute => "cs_6_8",
            _ => "lib_6_8"
        };
    }
}
