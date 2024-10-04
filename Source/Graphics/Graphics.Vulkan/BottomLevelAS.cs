using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class BottomLevelAS : VulkanObject<AccelerationStructureKHR>, IBindableResource
{
    internal BottomLevelAS(VulkanResources vkRes, ref readonly BottomLevelASDescription description) : base(vkRes, ObjectType.AccelerationStructureKhr)
    {
        AccelerationStructureGeometryKHR[] asGeometries = new AccelerationStructureGeometryKHR[description.Geometries.Length];
        AccelerationStructureBuildRangeInfoKHR[] asBuildRangeInfos = new AccelerationStructureBuildRangeInfoKHR[description.Geometries.Length];
        uint[] maxPrimitiveCounts = new uint[description.Geometries.Length];

        for (int i = 0; i < description.Geometries.Length; i++)
        {
            AccelerationStructureGeometry accelerationStructureGeometry = description.Geometries[i];

            AccelerationStructureGeometryKHR asGeometry;
            AccelerationStructureBuildRangeInfoKHR asBuildRangeInfo;

            if (accelerationStructureGeometry is AccelerationStructureTriangles triangles)
            {
                ulong vertexAddress = triangles.VertexBuffer.Address + triangles.VertexOffset;
                ulong indexAddress = triangles.IndexBuffer != null ? triangles.IndexBuffer.Address + triangles.IndexOffset : 0;

                asGeometry = new AccelerationStructureGeometryKHR
                {
                    SType = StructureType.AccelerationStructureGeometryKhr,
                    Flags = Formats.GetGeometryFlags(triangles.Options),
                    GeometryType = GeometryTypeKHR.TrianglesKhr,
                    Geometry = new AccelerationStructureGeometryDataKHR
                    {
                        Triangles = new AccelerationStructureGeometryTrianglesDataKHR
                        {
                            SType = StructureType.AccelerationStructureGeometryTrianglesDataKhr,
                            VertexData = new DeviceOrHostAddressConstKHR
                            {
                                DeviceAddress = vertexAddress
                            },
                            VertexFormat = Formats.GetPixelFormat(triangles.VertexFormat, false),
                            VertexStride = triangles.VertexStride,
                            MaxVertex = triangles.VertexCount,
                            IndexData = new DeviceOrHostAddressConstKHR
                            {
                                DeviceAddress = indexAddress
                            },
                            IndexType = triangles.IndexBuffer != null ? Formats.GetIndexType(triangles.IndexFormat) : IndexType.NoneKhr
                        }
                    }
                };

                asBuildRangeInfo = new AccelerationStructureBuildRangeInfoKHR
                {
                    PrimitiveCount = (triangles.IndexBuffer != null) ? (triangles.IndexCount / 3) : (triangles.VertexCount / 3),
                    PrimitiveOffset = 0,
                    FirstVertex = 0,
                    TransformOffset = 0
                };
            }
            else if (accelerationStructureGeometry is AccelerationStructureAABBs aabbs)
            {
                asGeometry = new AccelerationStructureGeometryKHR
                {
                    SType = StructureType.AccelerationStructureGeometryKhr,
                    Flags = Formats.GetGeometryFlags(aabbs.Options),
                    GeometryType = GeometryTypeKHR.AabbsKhr,
                    Geometry = new AccelerationStructureGeometryDataKHR
                    {
                        Aabbs = new AccelerationStructureGeometryAabbsDataKHR
                        {
                            SType = StructureType.AccelerationStructureGeometryAabbsDataKhr,
                            Data = new DeviceOrHostAddressConstKHR
                            {
                                DeviceAddress = aabbs.AABBs.Address
                            },
                            Stride = aabbs.Stride
                        }
                    }
                };

                asBuildRangeInfo = new AccelerationStructureBuildRangeInfoKHR
                {
                    PrimitiveCount = (uint)aabbs.Count,
                    PrimitiveOffset = aabbs.Offset,
                    FirstVertex = 0,
                    TransformOffset = 0
                };
            }
            else
            {
                throw new NotSupportedException("Geometry type not supported.");
            }

            asGeometries[i] = asGeometry;
            asBuildRangeInfos[i] = asBuildRangeInfo;
            maxPrimitiveCounts[i] = asBuildRangeInfo.PrimitiveCount;
        }

        AccelerationStructureBuildGeometryInfoKHR asBuildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr,
            GeometryCount = (uint)asGeometries.Length,
            PGeometries = asGeometries.AsPointer(),
            Flags = BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr
        };
        AccelerationStructureBuildSizesInfoKHR asBuildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        VkRes.KhrAccelerationStructure.GetAccelerationStructureBuildSizes(VkRes.VkDevice,
                                                                          AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                          &asBuildGeometryInfo,
                                                                          maxPrimitiveCounts.AsPointer(),
                                                                          &asBuildSizesInfo);

        DeviceBuffer asBuffer = new(VkRes,
                                    BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureStorageBitKhr,
                                    (uint)asBuildSizesInfo.AccelerationStructureSize,
                                    false);

        AccelerationStructureCreateInfoKHR asCreateInfo = new()
        {
            SType = StructureType.AccelerationStructureCreateInfoKhr,
            Buffer = asBuffer.Handle,
            Size = asBuildSizesInfo.AccelerationStructureSize,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr
        };

        AccelerationStructureKHR bottomLevelAS;
        VkRes.KhrAccelerationStructure.CreateAccelerationStructure(VkRes.VkDevice, &asCreateInfo, null, &bottomLevelAS).ThrowCode();

        AccelerationStructureDeviceAddressInfoKHR asDeviceAddressInfo = new()
        {
            SType = StructureType.AccelerationStructureDeviceAddressInfoKhr,
            AccelerationStructure = bottomLevelAS
        };

        ulong bottomLevelASAddress = VkRes.KhrAccelerationStructure.GetAccelerationStructureDeviceAddress(VkRes.VkDevice, &asDeviceAddressInfo);

        using DeviceBuffer scratchBuffer = new(VkRes,
                                               BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.StorageBufferBit,
                                               (uint)asBuildSizesInfo.BuildScratchSize,
                                               false);

        asBuildGeometryInfo.Mode = BuildAccelerationStructureModeKHR.BuildKhr;
        asBuildGeometryInfo.DstAccelerationStructure = bottomLevelAS;
        asBuildGeometryInfo.ScratchData = new DeviceOrHostAddressKHR
        {
            DeviceAddress = scratchBuffer.Address
        };

        using StagingCommandPool stagingCommandPool = new(VkRes, VkRes.GraphicsDevice.GraphicsExecutor);

        CommandBuffer commandBuffer = stagingCommandPool.BeginNewCommandBuffer();

        AccelerationStructureBuildRangeInfoKHR* pAsBuildRangeInfos = asBuildRangeInfos.AsPointer();

        VkRes.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandBuffer,
                                                                      1,
                                                                      &asBuildGeometryInfo,
                                                                      &pAsBuildRangeInfos);

        stagingCommandPool.EndAndSubmitCommandBuffer(commandBuffer);

        Handle = bottomLevelAS;
        Address = bottomLevelASAddress;
        DeviceBuffer = asBuffer;
    }

    internal override AccelerationStructureKHR Handle { get; }

    internal ulong Address { get; }

    internal DeviceBuffer DeviceBuffer { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        VkRes.KhrAccelerationStructure.DestroyAccelerationStructure(VkRes.VkDevice, Handle, null);

        DeviceBuffer.Dispose();
    }
}
