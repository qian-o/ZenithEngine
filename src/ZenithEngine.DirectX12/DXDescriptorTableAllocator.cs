using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDescriptorTableAllocator : GraphicsResource
{
    public ComPtr<ID3D12DescriptorHeap> CpuHeap;
    public ComPtr<ID3D12DescriptorHeap> GpuHeap;

    private readonly uint descriptorSize;
    private readonly CpuDescriptorHandle cpuStart;
    private readonly CpuDescriptorHandle gpuStart;

    private uint offset;
    private bool dirty;

    public DXDescriptorTableAllocator(GraphicsContext context,
                                      DescriptorHeapType heapType,
                                      uint numDescriptors) : base(context)
    {
        DescriptorHeapDesc cpuDesc = new()
        {
            Type = heapType,
            NumDescriptors = numDescriptors,
            Flags = DescriptorHeapFlags.None
        };

        DescriptorHeapDesc gpuDesc = new()
        {
            Type = heapType,
            NumDescriptors = numDescriptors,
            Flags = DescriptorHeapFlags.ShaderVisible
        };

        Context.Device.CreateDescriptorHeap(&cpuDesc, out CpuHeap).ThrowIfError();
        Context.Device.CreateDescriptorHeap(&gpuDesc, out GpuHeap).ThrowIfError();

        descriptorSize = Context.Device.GetDescriptorHandleIncrementSize(heapType);
        cpuStart = CpuHeap.GetCPUDescriptorHandleForHeapStart();
        gpuStart = GpuHeap.GetCPUDescriptorHandleForHeapStart();

        HeapType = heapType;
    }

    public DescriptorHeapType HeapType { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public CpuDescriptorHandle Alloc(CpuDescriptorHandle[] handles)
    {
        CpuDescriptorHandle table = new(gpuStart.Ptr + (offset * descriptorSize));

        foreach (CpuDescriptorHandle handle in handles)
        {
            Context.Device.CopyDescriptorsSimple(1,
                                                 new(cpuStart.Ptr + (offset * descriptorSize)),
                                                 handle,
                                                 HeapType);

            offset++;
        }

        dirty = true;

        return table;
    }

    public void Submit()
    {
        if (!dirty)
        {
            return;
        }

        Context.Device.CopyDescriptorsSimple(offset,
                                             GpuHeap.GetCPUDescriptorHandleForHeapStart(),
                                             CpuHeap.GetCPUDescriptorHandleForHeapStart(),
                                             HeapType);
    }

    public void Reset()
    {
        offset = 0;
        dirty = false;
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        GpuHeap.Dispose();
        CpuHeap.Dispose();
    }
}
