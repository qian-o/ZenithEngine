using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKBottomLevelAS : BottomLevelAS
{
    public VkAccelerationStructure AccelerationStructure;

    public VKBottomLevelAS(GraphicsContext context,
                           VkCommandBuffer commandBuffer,
                           ref readonly BottomLevelASDesc desc) : base(context, in desc)
    {
        TransformBuffer = new(Context,
                              (uint)(desc.Geometries.Length * sizeof(TransformMatrixKHR)),
                              BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                              true);

        MappedResource mapped = Context.MapMemory(TransformBuffer, MapMode.Write);

        for (int i = 0; i < desc.Geometries.Length; i++)
        {
            if (desc.Geometries[i] is AccelerationStructureTriangles triangles)
            {
                TransformMatrixKHR transformMatrix = VKFormats.GetTransformMatrix(triangles.Transform);

                Unsafe.Copy((byte*)(mapped.Data + i * sizeof(TransformMatrixKHR)), in transformMatrix);
            }
        }

        Context.UnmapMemory(TransformBuffer);

        AccelerationStructureGeometryKHR* geometries = Allocator.Alloc<AccelerationStructureGeometryKHR>((uint)desc.Geometries.Length);
        AccelerationStructureBuildRangeInfoKHR* buildRangeInfos = Allocator.Alloc<AccelerationStructureBuildRangeInfoKHR>((uint)desc.Geometries.Length);
        uint* primitiveCounts = Allocator.Alloc<uint>((uint)desc.Geometries.Length);

        for (int i = 0; i < desc.Geometries.Length; i++)
        {
            AccelerationStructureGeometryKHR geometry = new()
            {
                SType = StructureType.AccelerationStructureGeometryKhr,
                Flags = VKFormats.GetGeometryFlags(desc.Geometries[i].Options)
            };

            AccelerationStructureBuildRangeInfoKHR buildRangeInfo = new();

            if (desc.Geometries[i] is AccelerationStructureTriangles triangles)
            {
                geometry.GeometryType = GeometryTypeKHR.TrianglesKhr;
                geometry.Geometry = new()
                {
                    Triangles = new()
                    {
                        SType = StructureType.AccelerationStructureGeometryTrianglesDataKhr,
                        VertexFormat = VKFormats.GetPixelFormat(triangles.VertexFormat),
                        VertexData = new()
                        {
                            DeviceAddress = triangles.VertexBuffer.VK().Address + triangles.VertexOffsetInBytes
                        },
                        VertexStride = triangles.VertexStrideInBytes,
                        MaxVertex = triangles.VertexCount,
                        IndexType = triangles.IndexBuffer is not null ? VKFormats.GetIndexType(triangles.IndexFormat) : IndexType.NoneKhr,
                        IndexData = new()
                        {
                            DeviceAddress = triangles.IndexBuffer is not null ? triangles.IndexBuffer.VK().Address + triangles.IndexOffsetInBytes : 0
                        },
                        TransformData = new()
                        {
                            DeviceAddress = TransformBuffer.Address + (uint)(i * sizeof(TransformMatrixKHR))
                        }
                    }
                };

                buildRangeInfo.PrimitiveCount = triangles.IndexBuffer is not null ? triangles.IndexCount / 3 : triangles.VertexCount / 3;
            }
            else if (desc.Geometries[i] is AccelerationStructureAABBs aABBs)
            {
                geometry.GeometryType = GeometryTypeKHR.AabbsKhr;
                geometry.Geometry = new()
                {
                    Aabbs = new()
                    {
                        SType = StructureType.AccelerationStructureGeometryAabbsDataKhr,
                        Data = new()
                        {
                            DeviceAddress = aABBs.AABBs.VK().Address + aABBs.OffsetInBytes
                        },
                        Stride = aABBs.StrideInBytes
                    }
                };

                buildRangeInfo.PrimitiveCount = aABBs.Count;
            }
            else
            {
                throw new NotSupportedException();
            }

            geometries[i] = geometry;
            buildRangeInfos[i] = buildRangeInfo;
            primitiveCounts[i] = buildRangeInfo.PrimitiveCount;
        }
    }

    public VKBuffer TransformBuffer { get; }

    public VKBuffer AccelerationStructureBuffer { get; }

    public VKBuffer ScratchBuffer { get; }

    public ulong Address { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.AccelerationStructureKhr, AccelerationStructure.Handle, name);
    }

    protected override void Destroy()
    {
        Context.KhrAccelerationStructure!.DestroyAccelerationStructure(Context.Device, AccelerationStructure, null);

        TransformBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }
}
