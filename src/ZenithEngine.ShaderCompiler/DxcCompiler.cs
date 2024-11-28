using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace ZenithEngine.ShaderCompiler;

public static unsafe class DxcCompiler
{
    private const int DXC_CP_ACP = 0;

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

    public static ReadOnlySpan<byte> Compile(ShaderStages stage,
                                             string source,
                                             string entryPoint,
                                             Func<string, ReadOnlySpan<byte>>? includeHandler = null)
    {
        _ = includeHandler;

        using MemoryAllocator allocator = new();

        string[] arguments = GetArguments(stage, entryPoint);

        DxcBuffer buffer = new()
        {
            Ptr = allocator.AllocAnsi(source),
            Size = Utils.CalcAnsiSizeInBytes(source),
            Encoding = DXC_CP_ACP
        };

        DxcCompiler3.Compile(ref buffer,
                             (char**)allocator.AllocUni(arguments),
                             (uint)arguments.Length,
                             ref Unsafe.NullRef<IDxcIncludeHandler>(),
                             out ComPtr<IDxcResult> result);

        return [];
    }

    private static string[] GetArguments(ShaderStages stage, string entryPoint)
    {
        List<string> arguments = [];

        arguments.Add($"-T {GetProfile(stage)}");

        if (stage > ShaderStages.None && stage < ShaderStages.RayGeneration)
        {
            arguments.Add($"-E {entryPoint}");
        }

        arguments.Add("-Zpr");

        arguments.Add("-fvk-use-scalar-layout");

        arguments.Add("-fvk-t-shift");
        arguments.Add("20");
        arguments.Add("all");

        arguments.Add("-fvk-s-shift");
        arguments.Add("40");
        arguments.Add("all");

        arguments.Add("-fvk-u-shift");
        arguments.Add("60");
        arguments.Add("all");

        arguments.Add("-spirv");

        return [.. arguments];
    }

    private static string GetProfile(ShaderStages stage)
    {
        return stage switch
        {
            ShaderStages.Vertex => "vs_6_0",
            ShaderStages.Hull => "hs_6_0",
            ShaderStages.Domain => "ds_6_0",
            ShaderStages.Geometry => "gs_6_0",
            ShaderStages.Pixel => "ps_6_0",
            ShaderStages.Compute => "cs_6_0",
            _ => "lib_6_3"
        };
    }
}
