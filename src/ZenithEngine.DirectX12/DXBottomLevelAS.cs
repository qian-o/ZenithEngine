﻿using System.Runtime.CompilerServices;
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
                              ResourceFlags.None,
                              ResourceStates.NonPixelShaderResource);

        MappedResource mapped = Context.MapMemory(TransformBuffer, MapMode.Write);

        for (uint i = 0; i < geometryCount; i++)
        {
            if (desc.Geometries[i] is AccelerationStructureTriangles triangles)
            {
                Matrix3X4<float> matrix3X4 = DXFormats.GetMatrix3X4(triangles.Transform);

                Unsafe.Copy((void*)(mapped.Data + (i * sizeof(Matrix3X4<float>))), in matrix3X4);
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
    }

    public DXBuffer TransformBuffer { get; }

    public DXBuffer AccelerationStructureBuffer { get; }

    public DXBuffer ScratchBuffer { get; }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        TransformBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }
}
