﻿using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKBuffer : Buffer
{
    public VkBuffer Buffer;
    public ulong Address;

    public VKBuffer(GraphicsContext context,
                    ref readonly BufferDesc desc) : base(context, in desc)
    {
        BufferCreateInfo createInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = desc.SizeInBytes,
            Usage = BufferUsageFlags.TransferSrcBit
                    | BufferUsageFlags.TransferDstBit
                    | BufferUsageFlags.ShaderDeviceAddressBit,
            SharingMode = Context.SharingEnabled ? SharingMode.Concurrent : SharingMode.Exclusive
        };

        if (Context.SharingEnabled)
        {
            createInfo.QueueFamilyIndexCount = (uint)Context.QueueFamilyIndices!.Length;
            createInfo.PQueueFamilyIndices = Allocator.Alloc(Context.QueueFamilyIndices);
        }

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

        if (desc.Usage.HasFlag(BufferUsage.ShaderResource) || desc.Usage.HasFlag(BufferUsage.UnorderedAccess))
        {
            createInfo.Usage |= BufferUsageFlags.StorageBufferBit;
        }

        if (desc.Usage.HasFlag(BufferUsage.AccelerationStructure))
        {
            createInfo.Usage |= BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr;
        }

        if (desc.Usage.HasFlag(BufferUsage.IndirectBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.IndirectBufferBit;
        }

        Context.Vk.CreateBuffer(Context.Device, &createInfo, null, out Buffer).ThrowIfError();

        DeviceMemory = CreateDeviceMemory(desc.Usage.HasFlag(BufferUsage.Dynamic), out Address);

        Allocator.Release();
    }

    public VKBuffer(GraphicsContext context,
                    ref readonly BufferDesc desc,
                    bool isDynamic,
                    BufferUsageFlags usage) : base(context, in desc)
    {
        BufferCreateInfo createInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = desc.SizeInBytes,
            Usage = usage | BufferUsageFlags.ShaderDeviceAddressBit,
            SharingMode = Context.SharingEnabled ? SharingMode.Concurrent : SharingMode.Exclusive
        };

        if (Context.SharingEnabled)
        {
            createInfo.QueueFamilyIndexCount = (uint)Context.QueueFamilyIndices!.Length;
            createInfo.PQueueFamilyIndices = Allocator.Alloc(Context.QueueFamilyIndices);
        }

        Context.Vk.CreateBuffer(Context.Device, &createInfo, null, out Buffer).ThrowIfError();

        DeviceMemory = CreateDeviceMemory(isDynamic, out Address);

        Allocator.Release();
    }

    public VKDeviceMemory DeviceMemory { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void SetName(string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = ObjectType.Buffer,
            ObjectHandle = Buffer.Handle,
            PObjectName = Allocator.AllocUTF8(name)
        };

        Context.ExtDebugUtils!.SetDebugUtilsObjectName(Context.Device, &nameInfo).ThrowIfError();
    }

    protected override void Destroy()
    {
        DeviceMemory.Dispose();

        Context.Vk.DestroyBuffer(Context.Device, Buffer, null);
    }

    private VKDeviceMemory CreateDeviceMemory(bool isDynamic, out ulong address)
    {
        BufferMemoryRequirementsInfo2 requirementsInfo2 = new()
        {
            SType = StructureType.BufferMemoryRequirementsInfo2,
            Buffer = Buffer
        };

        MemoryRequirements2 requirements2 = new()
        {
            SType = StructureType.MemoryRequirements2
        };

        requirements2.AddNext(out MemoryDedicatedRequirements dedicatedRequirements);

        Context.Vk.GetBufferMemoryRequirements2(Context.Device, &requirementsInfo2, &requirements2);

        VKDeviceMemory deviceMemory = new(Context,
                                          isDynamic,
                                          requirements2.MemoryRequirements,
                                          dedicatedRequirements.PrefersDedicatedAllocation || dedicatedRequirements.RequiresDedicatedAllocation,
                                          null,
                                          Buffer);

        Context.Vk.BindBufferMemory(Context.Device,
                                    Buffer,
                                    deviceMemory.DeviceMemory,
                                    0).ThrowIfError();

        BufferDeviceAddressInfo addressInfo = new()
        {
            SType = StructureType.BufferDeviceAddressInfo,
            Buffer = Buffer
        };

        address = Context.Vk.GetBufferDeviceAddress(Context.Device, &addressInfo);

        return deviceMemory;
    }
}
