﻿using Silk.NET.Maths;
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
        BufferDesc instanceBufferDesc = new((uint)(desc.Instances.Length * sizeof(AccelerationStructureInstanceKHR)));

        InstanceBuffer = new(Context,
                             in instanceBufferDesc,
                             true,
                             BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr);

        FillInstanceBuffer(out AccelerationStructureGeometryKHR geometry, out AccelerationStructureBuildRangeInfoKHR buildRangeInfo);

        AccelerationStructureBuildGeometryInfoKHR buildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.TopLevelKhr,
            Mode = BuildAccelerationStructureModeKHR.BuildKhr,
            GeometryCount = 1,
            PGeometries = &geometry,
            Flags = VKFormats.GetBuildAccelerationStructureFlags(desc.Options)
        };

        AccelerationStructureBuildSizesInfoKHR buildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        Context.KhrAccelerationStructure!.GetAccelerationStructureBuildSizes(Context.Device,
                                                                             AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                             &buildGeometryInfo,
                                                                             &buildRangeInfo.PrimitiveCount,
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

    public void UpdateAccelerationStructure(VkCommandBuffer commandBuffer, ref readonly TopLevelASDesc newDesc)
    {
        Desc = newDesc;

        FillInstanceBuffer(out AccelerationStructureGeometryKHR geometry, out AccelerationStructureBuildRangeInfoKHR buildRangeInfo);

        AccelerationStructureBuildGeometryInfoKHR buildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.TopLevelKhr,
            Mode = BuildAccelerationStructureModeKHR.UpdateKhr,
            SrcAccelerationStructure = AccelerationStructure,
            DstAccelerationStructure = AccelerationStructure,
            GeometryCount = 1,
            PGeometries = &geometry,
            ScratchData = new()
            {
                DeviceAddress = ScratchBuffer.Address
            },
            Flags = VKFormats.GetBuildAccelerationStructureFlags(Desc.Options)
        };

        AccelerationStructureBuildRangeInfoKHR* pBuildRangeInfo = &buildRangeInfo;

        Context.KhrAccelerationStructure!.CmdBuildAccelerationStructures(commandBuffer, 1, &buildGeometryInfo, &pBuildRangeInfo);
    }

    protected override void SetName(string name)
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

        InstanceBuffer.Dispose();
        AccelerationStructureBuffer.Dispose();
        ScratchBuffer.Dispose();
    }

    private void FillInstanceBuffer(out AccelerationStructureGeometryKHR geometry, out AccelerationStructureBuildRangeInfoKHR buildRangeInfo)
    {
        uint instanceCount = (uint)Desc.Instances.Length;

        MappedResource mapped = Context.MapMemory(InstanceBuffer, MapMode.Write);

        Span<AccelerationStructureInstanceKHR> instances = new((void*)mapped.Data, (int)instanceCount);

        for (uint i = 0; i < instanceCount; i++)
        {
            AccelerationStructureInstance instance = Desc.Instances[i];

            instances[(int)i] = new()
            {
                InstanceCustomIndex = instance.InstanceID,
                Mask = instance.InstanceMask,
                InstanceShaderBindingTableRecordOffset = instance.InstanceContributionToHitGroupIndex,
                AccelerationStructureReference = instance.BottomLevel.VK().Address,
                Flags = VKFormats.GetGeometryInstanceFlags(instance.Options)
            };

            fixed (float* pMatrix = instances[(int)i].Transform.Matrix)
            {
                new Span<Matrix3X4<float>>(pMatrix, 1)[0] = instance.Transform;
            }
        }

        Context.UnmapMemory(InstanceBuffer);

        geometry = new()
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

        buildRangeInfo = new()
        {
            PrimitiveCount = instanceCount,
            PrimitiveOffset = Desc.OffsetInBytes,
            FirstVertex = 0,
            TransformOffset = 0
        };
    }
}
