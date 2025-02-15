using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXTopLevelAS : TopLevelAS
{
    private CpuDescriptorHandle srv;

    public DXTopLevelAS(GraphicsContext context,
                        ComPtr<ID3D12GraphicsCommandList4> commandList,
                        ref readonly TopLevelASDesc desc) : base(context, in desc)
    {
        BufferDesc instanceBufferDesc = new((uint)(desc.Instances.Length * sizeof(RaytracingInstanceDesc)));

        InstanceBuffer = new(Context,
                             in instanceBufferDesc,
                             HeapType.Upload,
                             ResourceFlags.None,
                             ResourceStates.GenericRead);

        FillInstanceBuffer(out BuildRaytracingAccelerationStructureInputs inputs);

        RaytracingAccelerationStructurePrebuildInfo buildInfo = new();

        Context.Device5.GetRaytracingAccelerationStructurePrebuildInfo(&inputs, &buildInfo);

        BufferDesc accelerationStructureBufferDesc = new((uint)buildInfo.ResultDataMaxSizeInBytes);

        AccelerationStructureBuffer = new(Context,
                                          in accelerationStructureBufferDesc,
                                          HeapType.Default,
                                          ResourceFlags.AllowUnorderedAccess,
                                          ResourceStates.RaytracingAccelerationStructure);

        BufferDesc scratchBufferDesc = new((uint)buildInfo.ScratchDataSizeInBytes);

        ScratchBuffer = new(Context,
                            in scratchBufferDesc,
                            HeapType.Default,
                            ResourceFlags.AllowUnorderedAccess,
                            ResourceStates.Common);

        BuildRaytracingAccelerationStructureDesc buildDesc = new()
        {
            DestAccelerationStructureData = AccelerationStructureBuffer.Resource.GetGPUVirtualAddress(),
            Inputs = inputs,
            ScratchAccelerationStructureData = ScratchBuffer.Resource.GetGPUVirtualAddress()
        };

        commandList.BuildRaytracingAccelerationStructure(&buildDesc, 0, (RaytracingAccelerationStructurePostbuildInfoDesc*)null);

        ResourceBarrier barrier = new()
        {
            Type = ResourceBarrierType.Uav,
            UAV = new()
            {
                PResource = AccelerationStructureBuffer.Resource
            }
        };

        commandList.ResourceBarrier(1, &barrier);

        Allocator.Release();
    }

    public DXBuffer InstanceBuffer { get; }

    public DXBuffer AccelerationStructureBuffer { get; }

    public DXBuffer ScratchBuffer { get; }

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

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        if (srv.Ptr is not 0)
        {
            Context.CbvSrvUavAllocator!.Free(srv);
        }

        InstanceBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }

    private void FillInstanceBuffer(out BuildRaytracingAccelerationStructureInputs inputs)
    {
        uint instanceCount = (uint)Desc.Instances.Length;

        MappedResource mapped = Context.MapMemory(InstanceBuffer, MapMode.Write);

        for (uint i = 0; i < instanceCount; i++)
        {
            AccelerationStructureInstance item = Desc.Instances[i];

            RaytracingInstanceDesc instance = new()
            {
                InstanceID = item.InstanceID,
                InstanceMask = item.InstanceMask,
                InstanceContributionToHitGroupIndex = item.InstanceContributionToHitGroupIndex,
                Flags = (uint)DXFormats.GetRaytracingInstanceFlags(item.Options),
                AccelerationStructure = item.BottomLevel.DX().AccelerationStructureBuffer.Resource.GetGPUVirtualAddress(),
            };

            new Span<Matrix3X4<float>>(instance.Transform, 1)[0] = item.Transform;

            Unsafe.Copy((void*)(mapped.Data + (i * sizeof(RaytracingInstanceDesc))), in instance);
        }

        Context.UnmapMemory(InstanceBuffer);

        inputs = new()
        {
            Type = RaytracingAccelerationStructureType.TopLevel,
            Flags = DXFormats.GetRaytracingAccelerationStructureBuildFlags(Desc.Options),
            NumDescs = instanceCount,
            DescsLayout = ElementsLayout.Array,
            InstanceDescs = InstanceBuffer.Resource.GetGPUVirtualAddress() + Desc.OffsetInBytes
        };
    }

    private void InitSrv()
    {
        ShaderResourceViewDesc desc = new()
        {
            Format = Format.FormatUnknown,
            ViewDimension = SrvDimension.RaytracingAccelerationStructure,
            Shader4ComponentMapping = DXGraphicsContext.DefaultShader4ComponentMapping,
            RaytracingAccelerationStructure = new()
            {
                Location = AccelerationStructureBuffer.Resource.GetGPUVirtualAddress()
            }
        };

        srv = Context.CbvSrvUavAllocator!.Alloc();

        Context.Device.CreateShaderResourceView((ID3D12Resource*)null, &desc, srv);
    }
}