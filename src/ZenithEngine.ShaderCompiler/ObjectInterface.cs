using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZenithEngine.Common;

namespace ZenithEngine.ShaderCompiler;

internal unsafe class ObjectInterface : DisposableObject
{
    public const int HResultOk = 0;
    public const int HResultNoInterface = -2147467262;
    public const int HResultUnspecifiedFailure = -2147467259;

    private readonly MemoryAllocator allocator = new();

    public Guid Guid;

    public ObjectHandle* Handle;

    public VTable* VTable;

    public nint AdditionalVTableSlots;

    public ObjectInterface(Guid guid, int additionalVTableSlots)
    {
        Guid = guid;

        Handle = allocator.Alloc<ObjectHandle>();
        VTable = (VTable*)allocator.Alloc((uint)(sizeof(VTable) + additionalVTableSlots * sizeof(nint)));
        AdditionalVTableSlots = ((nint)VTable) + sizeof(VTable);

        Handle->VTable = VTable;
        Handle->ManagedHandle = GCHandle.Alloc(this);

        VTable->QueryInterface = &QueryInterface;
        VTable->AddRef = &AddRef;
        VTable->RemoveRef = &RemoveRef;
    }

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
            ObjectInterface self = pSelf->GetObject<ObjectInterface>();
            
            if (pInterfaceId[0] == self.Guid)
            {
                pInterface[0] = self.Handle;

                return HResultOk;
            }

            return HResultNoInterface;
        }
        catch
        {
            return HResultUnspecifiedFailure;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static ulong AddRef(ObjectHandle* pSelf)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static ulong RemoveRef(ObjectHandle* pSelf)
    {
        return 0;
    }
}
