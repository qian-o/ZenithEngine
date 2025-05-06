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
        SizeInBytes = Utils.Align(desc.SizeInBytes, 256u);

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
            Flags = desc.Usage.HasFlag(BufferUsage.UnorderedAccess) ? ResourceFlags.AllowUnorderedAccess : ResourceFlags.None
        };

        HeapProperties heapProperties = new(HeapType.Default);
        ResourceStates initialResourceState = ResourceStates.Common;

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

    public DXBuffer(GraphicsContext context,
                    ref readonly BufferDesc desc,
                    HeapType heapType,
                    ResourceFlags flags,
                    ResourceStates initialResourceState) : base(context, in desc)
    {
        SizeInBytes = desc.SizeInBytes;

        HeapProperties heapProperties = new(heapType);

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
            Flags = flags
        };

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
                InitCbv();
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
                InitSrv();
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
                InitUav();
            }

            return ref uav;
        }
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

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

    protected override void SetName(string name)
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

    private void InitCbv()
    {
        ConstantBufferViewDesc desc = new()
        {
            BufferLocation = Resource.GetGPUVirtualAddress(),
            SizeInBytes = SizeInBytes
        };

        cbv = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateConstantBufferView(&desc, cbv);
    }

    private void InitSrv()
    {
        ShaderResourceViewDesc desc = new()
        {
            Format = Format.FormatUnknown,
            ViewDimension = SrvDimension.Buffer,
            Shader4ComponentMapping = DXGraphicsContext.DefaultShader4ComponentMapping,
            Buffer = new()
            {
                FirstElement = 0,
                NumElements = SizeInBytes / Desc.StructureStrideInBytes,
                StructureByteStride = Desc.StructureStrideInBytes,
                Flags = BufferSrvFlags.None
            }
        };

        srv = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateShaderResourceView(Resource, &desc, srv);
    }

    private void InitUav()
    {
        UnorderedAccessViewDesc desc = new()
        {
            Format = Format.FormatUnknown,
            ViewDimension = UavDimension.Buffer,
            Buffer = new()
            {
                FirstElement = 0,
                NumElements = SizeInBytes / Desc.StructureStrideInBytes,
                StructureByteStride = Desc.StructureStrideInBytes,
                CounterOffsetInBytes = 0,
                Flags = BufferUavFlags.None
            }
        };

        uav = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateUnorderedAccessView(Resource, (ID3D12Resource*)null, &desc, uav);
    }
}
