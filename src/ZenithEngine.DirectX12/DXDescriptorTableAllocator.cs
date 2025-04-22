using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDescriptorTableAllocator : GraphicsResource
{
    public ComPtr<ID3D12DescriptorHeap> CpuHeap;
    public ComPtr<ID3D12DescriptorHeap> GpuHeap;

    private readonly CpuDescriptorHandle cpuStart;
    private readonly GpuDescriptorHandle gpuStart;
    private readonly uint descriptorSize;

    private uint allocatedDescriptors;

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

        cpuStart = CpuHeap.GetCPUDescriptorHandleForHeapStart();
        gpuStart = GpuHeap.GetGPUDescriptorHandleForHeapStart();
        descriptorSize = Context.Device.GetDescriptorHandleIncrementSize(heapType);

        HeapType = heapType;
    }

    public DescriptorHeapType HeapType { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void UpdateDescriptor(CpuDescriptorHandle handle)
    {
        Context.Device.CopyDescriptorsSimple(1,
                                             new(cpuStart.Ptr + (allocatedDescriptors * descriptorSize)),
                                             handle,
                                             HeapType);

        allocatedDescriptors++;
    }

    public CpuDescriptorHandle UpdateDescriptorHandle()
    {
        CpuDescriptorHandle handle = new(cpuStart.Ptr + (allocatedDescriptors * descriptorSize));

        allocatedDescriptors++;

        return handle;
    }

    public GpuDescriptorHandle GetCurrentTableHandle()
    {
        return new(gpuStart.Ptr + (allocatedDescriptors * descriptorSize));
    }

    public void Submit()
    {
        if (allocatedDescriptors is 0)
        {
            return;
        }

        Context.Device.CopyDescriptorsSimple(allocatedDescriptors,
                                             GpuHeap.GetCPUDescriptorHandleForHeapStart(),
                                             CpuHeap.GetCPUDescriptorHandleForHeapStart(),
                                             HeapType);
    }

    public void Reset()
    {
        allocatedDescriptors = 0;
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        GpuHeap.Dispose();
        CpuHeap.Dispose();
    }
}
