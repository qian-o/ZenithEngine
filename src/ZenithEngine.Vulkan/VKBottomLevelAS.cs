using Silk.NET.Maths;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKBottomLevelAS : BottomLevelAS
{
    public VkAccelerationStructure AccelerationStructure;
    public ulong Address;

    public VKBottomLevelAS(GraphicsContext context,
                           VkCommandBuffer commandBuffer,
                           ref readonly BottomLevelASDesc desc) : base(context, in desc)
    {
        uint geometryCount = (uint)desc.Geometries.Length;

        BufferDesc transformBufferDesc = new((uint)(geometryCount * sizeof(TransformMatrixKHR)));

        TransformBuffer = new(Context,
                              in transformBufferDesc,
                              true,
                              BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr);

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

        AccelerationStructureGeometryKHR* geometries = Allocator.Alloc<AccelerationStructureGeometryKHR>(geometryCount);
        AccelerationStructureBuildRangeInfoKHR* buildRangeInfos = Allocator.Alloc<AccelerationStructureBuildRangeInfoKHR>(geometryCount);
        uint* maxPrimitiveCounts = Allocator.Alloc<uint>(geometryCount);

        for (uint i = 0; i < geometryCount; i++)
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
                        VertexFormat = VKFormats.GetFormat(triangles.VertexFormat),
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
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(desc.Geometries[i]));
            }

            geometries[i] = geometry;
            buildRangeInfos[i] = buildRangeInfo;
            maxPrimitiveCounts[i] = buildRangeInfo.PrimitiveCount;
        }

        AccelerationStructureBuildGeometryInfoKHR buildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr,
            Mode = BuildAccelerationStructureModeKHR.BuildKhr,
            GeometryCount = geometryCount,
            PGeometries = geometries,
            Flags = BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr
        };

        AccelerationStructureBuildSizesInfoKHR buildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        Context.KhrAccelerationStructure!.GetAccelerationStructureBuildSizes(Context.Device,
                                                                             AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                             &buildGeometryInfo,
                                                                             maxPrimitiveCounts,
                                                                             &buildSizesInfo);

        BufferDesc accelerationStructureBufferDesc = new((uint)buildSizesInfo.AccelerationStructureSize);

        AccelerationStructureBuffer = new(Context,
                                          in accelerationStructureBufferDesc,
                                          false,
                                          BufferUsageFlags.AccelerationStructureStorageBitKhr);

        AccelerationStructureCreateInfoKHR createInfo = new()
        {
            SType = StructureType.AccelerationStructureCreateInfoKhr,
            Buffer = AccelerationStructureBuffer.Buffer,
            Size = buildSizesInfo.AccelerationStructureSize,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr
        };

        Context.KhrAccelerationStructure.CreateAccelerationStructure(Context.Device,
                                                                     &createInfo,
                                                                     null,
                                                                     out AccelerationStructure).ThrowIfError();

        AccelerationStructureDeviceAddressInfoKHR deviceAddressInfo = new()
        {
            SType = StructureType.AccelerationStructureDeviceAddressInfoKhr,
            AccelerationStructure = AccelerationStructure
        };

        Address = Context.KhrAccelerationStructure.GetAccelerationStructureDeviceAddress(Context.Device, &deviceAddressInfo);

        BufferDesc scratchBufferDesc = new((uint)buildSizesInfo.BuildScratchSize);

        ScratchBuffer = new(Context,
                            in scratchBufferDesc,
                            false,
                            BufferUsageFlags.StorageBufferBit);

        buildGeometryInfo.DstAccelerationStructure = AccelerationStructure;
        buildGeometryInfo.ScratchData = new()
        {
            DeviceAddress = ScratchBuffer.Address
        };

        Context.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandBuffer, 1, &buildGeometryInfo, &buildRangeInfos);

        MemoryBarrier barrier = new()
        {
            SType = StructureType.MemoryBarrier,
            SrcAccessMask = AccessFlags.AccelerationStructureReadBitKhr | AccessFlags.AccelerationStructureWriteBitKhr,
            DstAccessMask = AccessFlags.AccelerationStructureReadBitKhr | AccessFlags.AccelerationStructureWriteBitKhr
        };

        Context.Vk.CmdPipelineBarrier(commandBuffer,
                                      PipelineStageFlags.AccelerationStructureBuildBitKhr,
                                      PipelineStageFlags.AccelerationStructureBuildBitKhr,
                                      DependencyFlags.None,
                                      1,
                                      &barrier,
                                      0,
                                      null,
                                      0,
                                      null);

        Allocator.Release();
    }

    public VKBuffer TransformBuffer { get; }

    public VKBuffer AccelerationStructureBuffer { get; }

    public VKBuffer ScratchBuffer { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = ObjectType.AccelerationStructureKhr,
            ObjectHandle = AccelerationStructure.Handle,
            PObjectName = Allocator.AllocUTF8(name)
        };

        Context.ExtDebugUtils!.SetDebugUtilsObjectName(Context.Device, &nameInfo).ThrowIfError();
    }

    protected override void Destroy()
    {
        Context.KhrAccelerationStructure!.DestroyAccelerationStructure(Context.Device, AccelerationStructure, null);

        TransformBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }
}
