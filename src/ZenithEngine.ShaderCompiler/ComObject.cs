using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZenithEngine.Common;

namespace ZenithEngine.ShaderCompiler;

public abstract unsafe class ComObject : DisposableObject
{
    public struct ObjectHandle
    {
        public nint LpVtbl;

        public GCHandle ManagedHandle;

        public readonly T GetObject<T>() where T : ComObject
        {
            return (T)ManagedHandle.Target!;
        }
    }

    public const int Ok = 0;
    public const int NoInterface = -2147467262;
    public const int UnspecifiedFailure = -2147467259;

    private readonly Guid guid;
    private readonly MemoryAllocator allocator;

    private volatile int refCount = 1;

    public ObjectHandle* Handle;

    protected ComObject(int additionalVTableSlots)
    {
        guid = Guid.ParseExact(GetType().GetCustomAttribute<GuidAttribute>()!.Value, "D");
        allocator = new();

        Handle = allocator.Alloc<ObjectHandle>();
        Handle->LpVtbl = (nint)allocator.Alloc((uint)((3 + additionalVTableSlots) * sizeof(nint)));
        Handle->ManagedHandle = GCHandle.Alloc(this);

        ((delegate* unmanaged[Stdcall]<ObjectHandle*, Guid*, void**, int>*)Handle->LpVtbl)[0] = &QueryInterface;
        ((delegate* unmanaged[Stdcall]<ObjectHandle*, ulong>*)Handle->LpVtbl + sizeof(nint))[0] = &AddRef;
        ((delegate* unmanaged[Stdcall]<ObjectHandle*, ulong>*)Handle->LpVtbl + (sizeof(nint) * 2))[0] = &RemoveRef;

        InitVTable();
    }

    protected nint GetVTableSlot(int slot)
    {
        return Handle->LpVtbl + ((slot + 3) * sizeof(nint));
    }

    protected abstract void InitVTable();

    protected override void Destroy()
    {
        Handle->ManagedHandle.Free();

        allocator.Dispose();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(ObjectHandle* pSelf, Guid* pInterfaceId, void** pInterface)
    {
        try
        {
            ComObject self = pSelf->GetObject<ComObject>();

            if (pInterfaceId[0] == self.guid)
            {
                pInterface[0] = self.Handle;

                return Ok;
            }

            return NoInterface;
        }
        catch
        {
            return UnspecifiedFailure;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static ulong AddRef(ObjectHandle* pSelf)
    {
        ComObject self = pSelf->GetObject<ComObject>();

        Interlocked.Add(ref self.refCount, 1);

        return (ulong)self.refCount;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static ulong RemoveRef(ObjectHandle* pSelf)
    {
        ComObject self = pSelf->GetObject<ComObject>();

        Interlocked.Add(ref self.refCount, -1);

        return (ulong)self.refCount;
    }
}
