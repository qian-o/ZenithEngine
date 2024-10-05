using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class TopLevelAS : VulkanObject<AccelerationStructureKHR>, IBindableResource
{
    internal TopLevelAS(VulkanResources vkRes, ref readonly TopLevelASDescription description) : base(vkRes, ObjectType.AccelerationStructureKhr)
    {
        AccelerationStructureInstanceKHR[] asInstances = new AccelerationStructureInstanceKHR[description.Instances.Length];

        for (int i = 0; i < description.Instances.Length; i++)
        {
            AccelerationStructureInstance asInstance = description.Instances[i];

            asInstances[i] = new()
            {
                Transform = Util.GetTransformMatrix(asInstance.Transform4x4),
                InstanceCustomIndex = asInstance.InstanceID,
                Mask = asInstance.InstanceMask,
                InstanceShaderBindingTableRecordOffset = asInstance.InstanceContributionToHitGroupIndex,
                AccelerationStructureReference = asInstance.BottonLevel.Handle.Handle,
                Flags = Formats.GetGeometryInstanceFlags(asInstance.Options)
            };
        }

        DeviceBuffer instanceBuffer = new(vkRes,
                                          BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                                          (uint)(sizeof(AccelerationStructureInstanceKHR) * description.Instances.Length),
                                          true);

        vkRes.GraphicsDevice.UpdateBuffer(instanceBuffer, asInstances);

        AccelerationStructureGeometryKHR asGeometry = new()
        {
            SType = StructureType.AccelerationStructureGeometryKhr,
            Flags = GeometryFlagsKHR.OpaqueBitKhr,
            GeometryType = GeometryTypeKHR.InstancesKhr,
            Geometry = new AccelerationStructureGeometryDataKHR
            {
                Instances = new AccelerationStructureGeometryInstancesDataKHR
                {
                    SType = StructureType.AccelerationStructureGeometryInstancesDataKhr,
                    ArrayOfPointers = false,
                    Data = new DeviceOrHostAddressConstKHR
                    {
                        DeviceAddress = instanceBuffer.Address
                    }
                }
            }
        };

        AccelerationStructureBuildGeometryInfoKHR asBuildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.TopLevelKhr,
            GeometryCount = 1,
            PGeometries = &asGeometry,
            Flags = Formats.GetBuildAccelerationStructureFlags(description.Options)
        };

        AccelerationStructureBuildRangeInfoKHR asBuildRangeInfo = new()
        {
            PrimitiveCount = (uint)description.Instances.Length,
            PrimitiveOffset = description.Offset,
            FirstVertex = 0,
            TransformOffset = 0
        };

        AccelerationStructureBuildSizesInfoKHR asBuildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        VkRes.KhrAccelerationStructure.GetAccelerationStructureBuildSizes(VkRes.VkDevice,
                                                                          AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                          &asBuildGeometryInfo,
                                                                          &asBuildRangeInfo.PrimitiveCount,
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
            Type = AccelerationStructureTypeKHR.TopLevelKhr
        };

        AccelerationStructureKHR topLevelAS;
        VkRes.KhrAccelerationStructure.CreateAccelerationStructure(VkRes.VkDevice, &asCreateInfo, null, &topLevelAS).ThrowCode();

        AccelerationStructureDeviceAddressInfoKHR asDeviceAddressInfo = new()
        {
            SType = StructureType.AccelerationStructureDeviceAddressInfoKhr,
            AccelerationStructure = topLevelAS
        };

        ulong topLevelASAddress = VkRes.KhrAccelerationStructure.GetAccelerationStructureDeviceAddress(VkRes.VkDevice, &asDeviceAddressInfo);

        using DeviceBuffer scratchBuffer = new(VkRes,
                                               BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.StorageBufferBit,
                                               (uint)asBuildSizesInfo.BuildScratchSize,
                                               false);

        asBuildGeometryInfo.Mode = BuildAccelerationStructureModeKHR.BuildKhr;
        asBuildGeometryInfo.DstAccelerationStructure = topLevelAS;
        asBuildGeometryInfo.ScratchData = new DeviceOrHostAddressKHR
        {
            DeviceAddress = scratchBuffer.Address
        };

        using StagingCommandPool stagingCommandPool = new(VkRes, VkRes.GraphicsDevice.GraphicsExecutor);

        CommandBuffer commandBuffer = stagingCommandPool.BeginNewCommandBuffer();

        AccelerationStructureBuildRangeInfoKHR* pAsBuildRangeInfos = asBuildRangeInfo.AsPointer();

        VkRes.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandBuffer,
                                                                      1,
                                                                      &asBuildGeometryInfo,
                                                                      &pAsBuildRangeInfos);

        stagingCommandPool.EndAndSubmitCommandBuffer(commandBuffer);

        Handle = topLevelAS;
        Address = topLevelASAddress;
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
