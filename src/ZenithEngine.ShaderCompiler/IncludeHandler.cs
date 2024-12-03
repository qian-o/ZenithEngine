using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using ZenithEngine.Common;

namespace ZenithEngine.ShaderCompiler;

[Guid("7f61fc7d-950d-467f-b3e3-3c02fb49187c")]
internal unsafe class IncludeHandler(Func<string, string>? includeHandler) : ComObject(1)
{
    public Func<string, string> Handler { get; } = includeHandler ?? (static (_) => string.Empty);

    public Dictionary<string, ComPtr<IDxcBlobEncoding>> IncludeCache { get; } = [];

    protected override void InitVTable()
    {
        ((delegate* unmanaged[Stdcall]<ObjectHandle*, nint, IDxcBlob**, int>*)GetVTableSlot(0))[0] = &LoadSource;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LoadSource(ObjectHandle* pSelf, nint pFileName, IDxcBlob** includeSourceBlob)
    {
        IncludeHandler self = pSelf->GetObject<IncludeHandler>();

        string fileName = Utils.PtrToStringUni(pFileName);

        if (!self.IncludeCache.TryGetValue(fileName, out ComPtr<IDxcBlobEncoding> blob))
        {
            string shader = self.Handler.Invoke(Utils.PtrToStringUni(pFileName));

            if (string.IsNullOrEmpty(shader))
            {
                throw new InvalidOperationException($"Include file '{fileName}' contains no data.");
            }

            byte[] source = Encoding.UTF8.GetBytes(shader);

            DxcCompiler.DxcUtils.CreateBlob(ref source[0],
                                            (uint)source.Length,
                                            DXC.CPUtf8,
                                            (IDxcBlobEncoding**)&blob);

            self.IncludeCache.Add(fileName, blob);
        }

        *includeSourceBlob = blob.QueryInterface<IDxcBlob>();

        return Ok;
    }

    protected override void Destroy()
    {
        foreach (ComPtr<IDxcBlobEncoding> blob in IncludeCache.Values)
        {
            blob.Dispose();
        }

        base.Destroy();
    }
}
