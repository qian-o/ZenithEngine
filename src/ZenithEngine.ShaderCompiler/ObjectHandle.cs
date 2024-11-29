using System.Runtime.InteropServices;

namespace ZenithEngine.ShaderCompiler;

internal unsafe struct ObjectHandle
{
    public VTable* VTable;

    public GCHandle ManagedHandle;

    public readonly T GetObject<T>() where T : ObjectInterface
    {
        return (T)ManagedHandle.Target!;
    }
}
