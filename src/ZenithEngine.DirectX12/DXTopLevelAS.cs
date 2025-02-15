using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXTopLevelAS : TopLevelAS
{
    public DXTopLevelAS(GraphicsContext context,
                        ComPtr<ID3D12GraphicsCommandList4> commandList,
                        ref readonly TopLevelASDesc desc) : base(context, in desc)
    {
        BufferDesc instanceBufferDesc = new((uint)(desc.Instances.Length * sizeof(RaytracingInstanceDesc)));

        InstanceBuffer = new(Context, in instanceBufferDesc);

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
    }

    public DXBuffer InstanceBuffer { get; }

    public DXBuffer AccelerationStructureBuffer { get; }

    public DXBuffer ScratchBuffer { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        InstanceBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }

    private void FillInstanceBuffer(out BuildRaytracingAccelerationStructureInputs inputs)
    {
        throw new NotImplementedException();
    }
}