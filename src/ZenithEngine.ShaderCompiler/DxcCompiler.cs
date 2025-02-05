using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using ZenithEngine.Common;
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

    public static byte[] Compile(string source,
                                 string[] arguments,
                                 Func<string, string>? includeHandler = null)
    {
        using IncludeHandler handler = new(includeHandler);

        using MemoryAllocator allocator = new();

        using ComPtr<IDxcResult> result = default;
        using ComPtr<IDxcBlobUtf8> eb = default;
        using ComPtr<IDxcBlob> rb = default;

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

            throw new ZenithEngineException(Utils.PtrToStringUTF8((nint)eb.GetBufferPointer()));
        }

        result.GetResult(rb.GetAddressOf());

        Reflection(result);

        return [.. new ReadOnlySpan<byte>(rb.GetBufferPointer(), (int)rb.GetBufferSize())];
    }

    private static void Reflection(ComPtr<IDxcResult> result)
    {
        using ComPtr<IDxcBlob> reflection = default;

        result.GetOutput(OutKind.Reflection, SilkMarshal.GuidPtrOf<IDxcBlob>(), (void**)reflection.GetAddressOf(), null);
    }
}
