using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXBuffer : Buffer
{
    public ComPtr<ID3D12Resource> Resource;

    private CpuDescriptorHandle cbv;
    private CpuDescriptorHandle srv;
    private CpuDescriptorHandle uav;

    public DXBuffer(GraphicsContext context,
                    ref readonly BufferDesc desc) : base(context, in desc)
    {
        SizeInBytes = Utils.AlignedSize(desc.SizeInBytes, 256u);

        ResourceDesc resourceDesc = new()
        {
            Dimension = ResourceDimension.Buffer,
            Alignment = 0,
            Width = SizeInBytes,
            Height = 1,
            DepthOrArraySize = 1,
            MipLevels = 1,
            Format = Format.FormatUnknown,
            SampleDesc = new(1, 0),
            Layout = TextureLayout.LayoutRowMajor,
            Flags = ResourceFlags.None
        };

        HeapProperties heapProperties = new(HeapType.Default);
        ResourceStates initialResourceState = ResourceStates.Common;

        if (desc.Usage.HasFlag(BufferUsage.StorageBufferReadWrite))
        {
            resourceDesc.Flags |= ResourceFlags.AllowUnorderedAccess;
        }

        if (desc.Usage.HasFlag(BufferUsage.Dynamic))
        {
            heapProperties = new(HeapType.Upload);
            initialResourceState = ResourceStates.GenericRead;
        }

        Context.Device.CreateCommittedResource(&heapProperties,
                                               HeapFlags.None,
                                               &resourceDesc,
                                               initialResourceState,
                                               null,
                                               out Resource).ThrowIfError();

        State = initialResourceState;
    }

    public uint SizeInBytes { get; }

    public ResourceStates State { get; private set; }

    public ref readonly CpuDescriptorHandle Cbv
    {
        get
        {
            if (cbv.Ptr is 0)
            {
                cbv = GetCbv(0, SizeInBytes);
            }

            return ref cbv;
        }
    }

    public ref readonly CpuDescriptorHandle Srv
    {
        get
        {
            if (srv.Ptr is 0)
            {
                srv = GetSrv(0, SizeInBytes);
            }

            return ref srv;
        }
    }

    public ref readonly CpuDescriptorHandle Uav
    {
        get
        {
            if (uav.Ptr is 0)
            {
                uav = GetUav(0, SizeInBytes);
            }

            return ref uav;
        }
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    public CpuDescriptorHandle GetCbv(uint offsetInBytes, uint sizeInBytes)
    {
        ConstantBufferViewDesc desc = new()
        {
            BufferLocation = Resource.GetGPUVirtualAddress() + offsetInBytes,
            SizeInBytes = sizeInBytes
        };

        CpuDescriptorHandle handle = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateConstantBufferView(&desc, handle);

        return handle;
    }

    public CpuDescriptorHandle GetSrv(uint offsetInBytes, uint sizeInBytes)
    {
        ShaderResourceViewDesc desc = new()
        {
            Format = Format.FormatUnknown,
            ViewDimension = SrvDimension.Buffer,
            Shader4ComponentMapping = DXGraphicsContext.DefaultShader4ComponentMapping,
            Buffer = new()
            {
                FirstElement = offsetInBytes / Desc.StructureStrideInBytes,
                NumElements = sizeInBytes / Desc.StructureStrideInBytes,
                StructureByteStride = Desc.StructureStrideInBytes,
                Flags = BufferSrvFlags.None
            }
        };

        CpuDescriptorHandle handle = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateShaderResourceView(Resource, &desc, handle);

        return handle;
    }

    public CpuDescriptorHandle GetUav(uint offsetInBytes, uint sizeInBytes)
    {
        UnorderedAccessViewDesc desc = new()
        {
            Format = Format.FormatUnknown,
            ViewDimension = UavDimension.Buffer,
            Buffer = new()
            {
                FirstElement = offsetInBytes / Desc.StructureStrideInBytes,
                NumElements = sizeInBytes / Desc.StructureStrideInBytes,
                StructureByteStride = Desc.StructureStrideInBytes,
                CounterOffsetInBytes = 0,
                Flags = BufferUavFlags.None
            }
        };

        CpuDescriptorHandle handle = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateUnorderedAccessView(Resource, (ID3D12Resource*)null, &desc, handle);

        return handle;
    }

    public void TransitionState(ComPtr<ID3D12GraphicsCommandList> commandList,
                                ResourceStates newState)
    {
        if (State == newState)
        {
            return;
        }

        ResourceBarrier barrier = new()
        {
            Type = ResourceBarrierType.Transition,
            Transition = new()
            {
                PResource = Resource,
                Subresource = 0,
                StateBefore = State,
                StateAfter = newState
            }
        };

        commandList.ResourceBarrier(1, &barrier);

        State = newState;
    }

    protected override void DebugName(string name)
    {
        Resource.SetName(name).ThrowIfError();
    }

    protected override void Destroy()
    {
        if (uav.Ptr is not 0)
        {
            Context.CbvSrvUavAllocator!.Free(uav);
        }

        if (srv.Ptr is not 0)
        {
            Context.CbvSrvUavAllocator!.Free(srv);
        }

        if (cbv.Ptr is not 0)
        {
            Context.CbvSrvUavAllocator!.Free(cbv);
        }

        Resource.Dispose();
    }
}
