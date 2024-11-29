using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using ZenithEngine.Common;

namespace ZenithEngine.ShaderCompiler;

[Guid("7f61fc7d-950d-467f-b3e3-3c02fb49187c")]
internal unsafe class IncludeHandler(Func<string, byte[]>? includeHandler) : ComObject(1)
{
    public Func<string, byte[]> Handler { get; } = includeHandler ?? ((_) => []);

    protected override void InitVTable()
    {
        ((delegate* unmanaged[Stdcall]<ObjectHandle*, nint, IDxcBlob**, int>*)GetVTableSlot(0))[0] = &LoadSource;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LoadSource(ObjectHandle* pSelf, nint pFileName, IDxcBlob** includeSourceBlob)
    {
        byte[] data = pSelf->GetObject<IncludeHandler>().Handler.Invoke(Utils.PtrToStringUni(pFileName));

        ComPtr<IDxcBlobEncoding> blob = default;
        DxcCompiler.DxcUtils.CreateBlob(ref data[0], (uint)data.Length, DXC.CPUtf8, ref blob);

        includeSourceBlob[0] = (IDxcBlob*)(IDxcBlobEncoding*)blob;

        return Ok;
    }
}
