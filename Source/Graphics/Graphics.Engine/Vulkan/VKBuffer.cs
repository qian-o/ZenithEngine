﻿using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Exceptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;
using System.Runtime.CompilerServices;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKBuffer : Buffer
{
    public VKBuffer(Context context,
                    ref readonly BufferDesc desc) : base(context, in desc)
    {
        BufferCreateInfo createInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = desc.SizeInBytes,
            Usage = BufferUsageFlags.TransferSrcBit
                    | BufferUsageFlags.TransferDstBit
                    | BufferUsageFlags.ShaderDeviceAddressBit,
            SharingMode = SharingMode.Exclusive
        };

        if (desc.Usage.HasFlag(BufferUsage.VertexBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.VertexBufferBit;
        }

        if (desc.Usage.HasFlag(BufferUsage.IndexBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.IndexBufferBit;
        }

        if (desc.Usage.HasFlag(BufferUsage.ConstantBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.UniformBufferBit;
        }

        if (desc.Usage.HasFlag(BufferUsage.StorageBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.StorageBufferBit;
        }

        if (desc.Usage.HasFlag(BufferUsage.IndirectBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.IndirectBufferBit;
        }

        if (desc.Usage.HasFlag(BufferUsage.AccelerationStructure))
        {
            createInfo.Usage |= BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr;
        }

        VkBuffer buffer;
        Context.Vk.CreateBuffer(Context.Device, &createInfo, null, &buffer).ThrowCode();

        MemoryRequirements memoryRequirements;
        Context.Vk.GetBufferMemoryRequirements(Context.Device, buffer, &memoryRequirements);

        DeviceMemory = new(Context, desc.Usage.HasFlag(BufferUsage.Dynamic), memoryRequirements);

        Context.Vk.BindBufferMemory(Context.Device, buffer, DeviceMemory.DeviceMemory, 0).ThrowCode();

        BufferDeviceAddressInfo bufferDeviceAddressInfo = new()
        {
            SType = StructureType.BufferDeviceAddressInfo,
            Buffer = buffer
        };

        Address = Context.Vk.GetBufferDeviceAddress(Context.Device, &bufferDeviceAddressInfo);

        Buffer = buffer;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkBuffer Buffer { get; }

    public VKDeviceMemory DeviceMemory { get; }

    public ulong Address { get; }

    public void SetData(VkCommandBuffer commandBuffer,
                        nint source,
                        uint sourceSizeInBytes,
                        uint destinationOffsetInBytes = 0)
    {
        if (sourceSizeInBytes + destinationOffsetInBytes > Desc.SizeInBytes)
        {
            throw new BackendException("Source size is too large.");
        }

        if (Desc.Usage.HasFlag(BufferUsage.Dynamic))
        {
            MappedResource mappedResource = Context.MapMemory(this, MapMode.Write);

            Unsafe.CopyBlock((void*)(mappedResource.Data + destinationOffsetInBytes),
                             (void*)source,
                             sourceSizeInBytes);

            Context.UnmapMemory(this);
        }
        else
        {
            // TODO: Implement
        }
    }

    public void CopyTo(VkCommandBuffer commandBuffer,
                       VKBuffer destination,
                       uint size,
                       uint sourceOffset = 0,
                       uint destinationOffset = 0)
    {
        // TODO: Implement
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Buffer, Buffer.Handle, name);

        DeviceMemory.Name = name;
    }

    protected override void Destroy()
    {
        DeviceMemory.Dispose();

        Context.Vk.DestroyBuffer(Context.Device, Buffer, null);
    }
}
