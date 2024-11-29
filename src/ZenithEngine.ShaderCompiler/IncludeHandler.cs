using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Direct3D.Compilers;
using ZenithEngine.Common;

namespace ZenithEngine.ShaderCompiler;

[Guid("7f61fc7d-950d-467f-b3e3-3c02fb49187c")]
internal unsafe class IncludeHandler(Func<string, string>? includeHandler) : ComObject(1)
{
    public Func<string, string> Handler { get; } = includeHandler ?? ((_) => string.Empty);

    protected override void InitVTable()
    {
        ((delegate* unmanaged[Stdcall]<ObjectHandle*, nint, IDxcBlob**, int>*)GetVTableSlot(0))[0] = &LoadSource;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LoadSource(ObjectHandle* pSelf, nint pFileName, IDxcBlob** includeSourceBlob)
    {
        string shader = pSelf->GetObject<IncludeHandler>().Handler.Invoke(Utils.PtrToStringUni(pFileName));

        byte[] source = Encoding.UTF8.GetBytes(shader);

        DxcCompiler.DxcUtils.CreateBlob(ref source[0],
                                        (uint)source.Length,
                                        DXC.CPUtf8,
                                        (IDxcBlobEncoding**)&includeSourceBlob[0]);

        return Ok;
    }
}
