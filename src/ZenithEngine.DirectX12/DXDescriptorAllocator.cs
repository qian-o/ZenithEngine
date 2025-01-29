using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal class DXDescriptorAllocator : GraphicsResource
{
    public ComPtr<ID3D12DescriptorHeap> Heap;

    private readonly uint descriptorSize;
    private readonly bool[] descriptorUsed;
    private readonly Lock @lock;

    private CpuDescriptorHandle cpuStart;

    public DXDescriptorAllocator(GraphicsContext context,
                                 DescriptorHeapType heapType,
                                 uint count) : base(context)
    {
        DescriptorHeapDesc desc = new()
        {
            Type = heapType,
            NumDescriptors = count,
            Flags = DescriptorHeapFlags.None,
            NodeMask = 0
        };

        Context.Device.CreateDescriptorHeap(in desc, out Heap).ThrowIfError();

        descriptorSize = Context.Device.GetDescriptorHandleIncrementSize(heapType);
        descriptorUsed = new bool[count];
        @lock = new();

        cpuStart = Heap.GetCPUDescriptorHandleForHeapStart();
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public CpuDescriptorHandle Alloc()
    {
        using Lock.Scope _ = @lock.EnterScope();

        for (int i = 0; i < descriptorUsed.Length; i++)
        {
            if (!descriptorUsed[i])
            {
                descriptorUsed[i] = true;

                return new(cpuStart.Ptr + (nuint)(i * descriptorSize));
            }
        }

        throw new ZenithEngineException("Descriptor allocator is full.");
    }

    public void Free(CpuDescriptorHandle handle)
    {
        using Lock.Scope _ = @lock.EnterScope();

        int index = (int)((handle.Ptr - cpuStart.Ptr) / descriptorSize);

        if (index < 0 || index >= descriptorUsed.Length)
        {
            throw new ZenithEngineException("Invalid descriptor handle.");
        }

        descriptorUsed[index] = false;
    }

    protected override void DebugName(string name)
    {
        Heap.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        Heap.Dispose();
    }
}
