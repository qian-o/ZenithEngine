using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDescriptorTableAllocator : GraphicsResource
{
    public ComPtr<ID3D12DescriptorHeap> CpuHeap;
    public ComPtr<ID3D12DescriptorHeap> GpuHeap;

    private readonly uint descriptorSize;
    private readonly CpuDescriptorHandle[] cpuHandles;

    private uint currentOffset;
    private bool isDirty;

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
        cpuHandles = new CpuDescriptorHandle[numDescriptors];

        HeapType = heapType;
    }

    public DescriptorHeapType HeapType { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        GpuHeap.Dispose();
        CpuHeap.Dispose();
    }
}
