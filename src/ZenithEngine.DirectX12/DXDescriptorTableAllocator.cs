﻿using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDescriptorTableAllocator : GraphicsResource
{
    public ComPtr<ID3D12DescriptorHeap> Heap;

    private readonly CpuDescriptorHandle cpuStart;
    private readonly GpuDescriptorHandle gpuStart;
    private readonly uint descriptorSize;

    private uint allocatedDescriptors;

    public DXDescriptorTableAllocator(GraphicsContext context,
                                      DescriptorHeapType heapType,
                                      uint numDescriptors) : base(context)
    {
        DescriptorHeapDesc desc = new()
        {
            Type = heapType,
            NumDescriptors = numDescriptors,
            Flags = DescriptorHeapFlags.ShaderVisible
        };

        Context.Device.CreateDescriptorHeap(&desc, out Heap).ThrowIfError();

        cpuStart = Heap.GetCPUDescriptorHandleForHeapStart();
        gpuStart = Heap.GetGPUDescriptorHandleForHeapStart();
        descriptorSize = Context.Device.GetDescriptorHandleIncrementSize(heapType);

        HeapType = heapType;
    }

    public DescriptorHeapType HeapType { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public void UpdateDescriptors(CpuDescriptorHandle[] handles)
    {
        CpuDescriptorHandle dest = new(cpuStart.Ptr + (allocatedDescriptors * descriptorSize));

        foreach (CpuDescriptorHandle handle in handles)
        {
            Context.Device.CopyDescriptorsSimple(1, dest, handle, HeapType);

            dest.Ptr += descriptorSize;
        }

        allocatedDescriptors += (uint)handles.Length;
    }

    public GpuDescriptorHandle GetCurrentTableHandle()
    {
        return new(gpuStart.Ptr + (allocatedDescriptors * descriptorSize));
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
        Heap.Dispose();
    }
}
