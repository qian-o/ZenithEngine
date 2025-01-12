using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKTopLevelAS : TopLevelAS
{
    public VkAccelerationStructure AccelerationStructure;
    public ulong Address;

    public VKTopLevelAS(GraphicsContext context,
                        VkCommandBuffer commandBuffer,
                        ref readonly TopLevelASDesc desc) : base(context, in desc)
    {
        uint instanceCount = (uint)desc.Instances.Length;

        InstanceBuffer = new(Context,
                             (uint)(instanceCount * sizeof(AccelerationStructureInstanceKHR)),
                             BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                             true);

        MappedResource mapped = Context.MapMemory(InstanceBuffer, MapMode.Write);

        for (uint i = 0; i < instanceCount; i++)
        {
            AccelerationStructureInstance item = desc.Instances[i];

            AccelerationStructureInstanceKHR instance = new()
            {
                Transform = VKFormats.GetTransformMatrix(item.Transform),
                InstanceCustomIndex = item.InstanceID,
                Mask = item.InstanceMask,
                InstanceShaderBindingTableRecordOffset = item.InstanceContributionToHitGroupIndex,
                AccelerationStructureReference = item.BottomLevel.VK().Address,
                Flags = VKFormats.GetGeometryInstanceFlags(item.Options)
            };

            Unsafe.Copy((byte*)(mapped.Data + (i * sizeof(AccelerationStructureInstanceKHR))), in instance);
        }

        Context.UnmapMemory(InstanceBuffer);

        AccelerationStructureGeometryKHR geometry = new()
        {
            SType = StructureType.AccelerationStructureGeometryKhr,
            GeometryType = GeometryTypeKHR.InstancesKhr,
            Geometry = new()
            {
                Instances = new()
                {
                    SType = StructureType.AccelerationStructureGeometryInstancesDataKhr,
                    Data = new()
                    {
                        DeviceAddress = InstanceBuffer.Address
                    }
                }
            },
            Flags = GeometryFlagsKHR.OpaqueBitKhr
        };

        AccelerationStructureBuildRangeInfoKHR buildRangeInfo = new()
        {
            PrimitiveCount = instanceCount,
            PrimitiveOffset = desc.OffsetInBytes,
            FirstVertex = 0,
            TransformOffset = 0
        };

        AccelerationStructureBuildGeometryInfoKHR buildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.TopLevelKhr,
            GeometryCount = 1,
            PGeometries = &geometry,
            Mode = BuildAccelerationStructureModeKHR.BuildKhr,
            Flags = VKFormats.GetBuildAccelerationStructureFlags(desc.Options)
        };

        AccelerationStructureBuildSizesInfoKHR buildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        Context.KhrAccelerationStructure!.GetAccelerationStructureBuildSizes(Context.Device,
                                                                             AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                             &buildGeometryInfo,
                                                                             &instanceCount,
                                                                             &buildSizesInfo);

        AccelerationStructureBuffer = new(Context,
                                          (uint)buildSizesInfo.AccelerationStructureSize,
                                          BufferUsageFlags.AccelerationStructureStorageBitKhr,
                                          false);

        AccelerationStructureCreateInfoKHR createInfo = new()
        {
            SType = StructureType.AccelerationStructureCreateInfoKhr,
            Buffer = AccelerationStructureBuffer.Buffer,
            Size = buildSizesInfo.AccelerationStructureSize,
            Type = AccelerationStructureTypeKHR.TopLevelKhr
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

        ScratchBuffer = new(Context, (uint)buildSizesInfo.BuildScratchSize, BufferUsageFlags.StorageBufferBit, false);

        buildGeometryInfo.DstAccelerationStructure = AccelerationStructure;
        buildGeometryInfo.ScratchData = new()
        {
            DeviceAddress = ScratchBuffer.Address
        };

        AccelerationStructureBuildRangeInfoKHR* pBuildRangeInfo = &buildRangeInfo;

        Context.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandBuffer, 1, &buildGeometryInfo, &pBuildRangeInfo);

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
    }

    public VKBuffer InstanceBuffer { get; }

    public VKBuffer AccelerationStructureBuffer { get; }

    public VKBuffer ScratchBuffer { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.AccelerationStructureKhr, AccelerationStructure.Handle, name);
    }

    protected override void Destroy()
    {
        Context.KhrAccelerationStructure!.DestroyAccelerationStructure(Context.Device, AccelerationStructure, null);

        InstanceBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }
}
