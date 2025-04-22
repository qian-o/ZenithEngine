using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXBottomLevelAS : BottomLevelAS
{
    public DXBottomLevelAS(GraphicsContext context,
                           ComPtr<ID3D12GraphicsCommandList4> commandList,
                           ref readonly BottomLevelASDesc desc) : base(context, in desc)
    {
        uint geometryCount = (uint)desc.Geometries.Length;

        BufferDesc transformBufferDesc = new((uint)(geometryCount * sizeof(Matrix3X4<float>)));

        TransformBuffer = new(Context,
                              in transformBufferDesc,
                              HeapType.Upload,
                              ResourceFlags.None,
                              ResourceStates.GenericRead);

        MappedResource mapped = Context.MapMemory(TransformBuffer, MapMode.Write);

        Span<Matrix3X4<float>> transforms = new((void*)mapped.Data, (int)geometryCount);

        for (uint i = 0; i < geometryCount; i++)
        {
            if (desc.Geometries[i] is AccelerationStructureTriangles triangles)
            {
                transforms[(int)i] = triangles.Transform;
            }
        }

        Context.UnmapMemory(TransformBuffer);

        RaytracingGeometryDesc* geometries = Allocator.Alloc<RaytracingGeometryDesc>(geometryCount);

        for (uint i = 0; i < geometryCount; i++)
        {
            RaytracingGeometryDesc geometry = new()
            {
                Type = RaytracingGeometryType.Triangles,
                Flags = DXFormats.GetRaytracingGeometryFlags(desc.Geometries[i].Options)
            };

            if (desc.Geometries[i] is AccelerationStructureTriangles triangles)
            {
                geometry.Type = RaytracingGeometryType.Triangles;
                geometry.Triangles = new()
                {
                    Transform3x4 = TransformBuffer.Resource.GetGPUVirtualAddress() + (uint)(i * sizeof(Matrix3X4<float>)),
                    IndexFormat = DXFormats.GetFormat(triangles.IndexFormat),
                    VertexFormat = triangles.IndexBuffer is not null ? DXFormats.GetFormat(triangles.VertexFormat) : Format.FormatUnknown,
                    IndexCount = triangles.IndexCount,
                    VertexCount = triangles.VertexCount,
                    IndexBuffer = triangles.IndexBuffer is not null ? triangles.IndexBuffer.DX().Resource.GetGPUVirtualAddress() + triangles.IndexOffsetInBytes : 0,
                    VertexBuffer = new()
                    {
                        StartAddress = triangles.VertexBuffer.DX().Resource.GetGPUVirtualAddress() + triangles.VertexOffsetInBytes,
                        StrideInBytes = triangles.VertexStrideInBytes
                    }
                };
            }
            else if (desc.Geometries[i] is AccelerationStructureAABBs aABBs)
            {
                geometry.Type = RaytracingGeometryType.ProceduralPrimitiveAabbs;
                geometry.AABBs = new()
                {
                    AABBCount = aABBs.Count,
                    AABBs = new()
                    {
                        StartAddress = aABBs.AABBs.DX().Resource.GetGPUVirtualAddress() + aABBs.OffsetInBytes,
                        StrideInBytes = aABBs.StrideInBytes
                    }
                };
            }
            else
            {
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(desc.Geometries[i]));
            }

            geometries[i] = geometry;
        }

        BuildRaytracingAccelerationStructureInputs inputs = new()
        {
            Type = RaytracingAccelerationStructureType.BottomLevel,
            Flags = RaytracingAccelerationStructureBuildFlags.None,
            NumDescs = geometryCount,
            DescsLayout = ElementsLayout.Array,
            PGeometryDescs = geometries
        };

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

    public DXBuffer TransformBuffer { get; }

    public DXBuffer AccelerationStructureBuffer { get; }

    public DXBuffer ScratchBuffer { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        TransformBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }
}
