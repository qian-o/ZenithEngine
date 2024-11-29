
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using ZenithEngine.Common;

namespace ZenithEngine.ShaderCompiler;

internal unsafe class IncludeHandler : ObjectInterface
{
    private readonly Func<string, byte[]> handler;

    public IncludeHandler(Func<string, byte[]>? includeHandler) : base(new("7f61fc7d-950d-467f-b3e3-3c02fb49187c"), 1)
    {
        handler = includeHandler ?? (_ => []);

        ((delegate* unmanaged[Stdcall]<ObjectHandle*, nint, IDxcBlob**, int>*)AdditionalVTableSlots)[0] = &LoadSource;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LoadSource(ObjectHandle* pSelf, nint pFileName, IDxcBlob** includeSourceBlob)
    {
        try
        {
            IncludeHandler self = pSelf->GetObject<IncludeHandler>();

            byte[] data = self.handler.Invoke(Utils.PtrToStringUni(pFileName));

            ComPtr<IDxcBlobEncoding> blob = default;
            DxcCompiler.DxcUtils.CreateBlob(ref data[0], (uint)data.Length, DXC.CPUtf8, ref blob);

            includeSourceBlob[0] = (IDxcBlob*)(IDxcBlobEncoding*)blob;

            return HResultOk;
        }
        catch
        {
            return HResultUnspecifiedFailure;
        }
    }
}
