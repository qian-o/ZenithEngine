using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class DeviceBuffer : VulkanObject<VkBuffer>, IBindableResource
{
    internal DeviceBuffer(VulkanResources vkRes, ref readonly BufferDescription description) : base(vkRes, ObjectType.Buffer)
    {
        BufferUsageFlags bufferUsageFlags = BufferUsageFlags.TransferSrcBit | BufferUsageFlags.TransferDstBit;

        if (description.Usage.HasFlag(BufferUsage.VertexBuffer))
        {
            bufferUsageFlags |= BufferUsageFlags.VertexBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.IndexBuffer))
        {
            bufferUsageFlags |= BufferUsageFlags.IndexBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.UniformBuffer))
        {
            bufferUsageFlags |= BufferUsageFlags.UniformBufferBit
                                | BufferUsageFlags.ShaderDeviceAddressBit;
        }

        if (description.Usage.HasFlag(BufferUsage.StorageBuffer))
        {
            bufferUsageFlags |= BufferUsageFlags.StorageBufferBit
                                | BufferUsageFlags.ShaderDeviceAddressBit;
        }

        if (description.Usage.HasFlag(BufferUsage.IndirectBuffer))
        {
            bufferUsageFlags |= BufferUsageFlags.IndirectBufferBit;
        }

        BufferCreateInfo createInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = description.SizeInBytes,
            Usage = bufferUsageFlags,
            SharingMode = SharingMode.Exclusive
        };

        VkBuffer buffer;
        VkRes.Vk.CreateBuffer(VkRes.GraphicsDevice.Handle, &createInfo, null, &buffer).ThrowCode();

        MemoryRequirements memoryRequirements;
        VkRes.Vk.GetBufferMemoryRequirements(VkRes.GraphicsDevice.Handle, buffer, &memoryRequirements);

        bool isHostVisible = description.Usage.HasFlag(BufferUsage.Dynamic) || description.Usage.HasFlag(BufferUsage.Staging);
        bool isAddress = bufferUsageFlags.HasFlag(BufferUsageFlags.ShaderDeviceAddressBit);

        DeviceMemory deviceMemory = new(VkRes,
                                        in memoryRequirements,
                                        isHostVisible ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit,
                                        isAddress);

        VkRes.Vk.BindBufferMemory(VkRes.GraphicsDevice.Handle, buffer, deviceMemory.Handle, 0).ThrowCode();

        ulong address = 0;
        if (isAddress)
        {
            BufferDeviceAddressInfo bufferDeviceAddressInfo = new()
            {
                SType = StructureType.BufferDeviceAddressInfo,
                Buffer = buffer
            };

            address = VkRes.Vk.GetBufferDeviceAddress(VkRes.GraphicsDevice.Handle, &bufferDeviceAddressInfo);
        }

        Handle = buffer;
        DeviceMemory = deviceMemory;
        Address = address;
        SizeInBytes = description.SizeInBytes;
        Usage = description.Usage;
        IsHostVisible = isHostVisible;
    }

    internal DeviceBuffer(VulkanResources vkRes,
                          uint sizeInBytes,
                          bool isDescriptorBuffer,
                          bool isDynamicBuffer) : base(vkRes, ObjectType.Buffer)
    {
        BufferUsageFlags bufferUsageFlags = BufferUsageFlags.TransferSrcBit | BufferUsageFlags.TransferDstBit;

        if (isDescriptorBuffer)
        {
            bufferUsageFlags |= BufferUsageFlags.ResourceDescriptorBufferBitExt
                                | BufferUsageFlags.SamplerDescriptorBufferBitExt
                                | BufferUsageFlags.ShaderDeviceAddressBit;
        }


        BufferCreateInfo createInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = sizeInBytes,
            Usage = bufferUsageFlags,
            SharingMode = SharingMode.Exclusive
        };

        VkBuffer buffer;
        VkRes.Vk.CreateBuffer(VkRes.GraphicsDevice.Handle, &createInfo, null, &buffer).ThrowCode();

        MemoryRequirements memoryRequirements;
        VkRes.Vk.GetBufferMemoryRequirements(VkRes.GraphicsDevice.Handle, buffer, &memoryRequirements);

        DeviceMemory deviceMemory = new(VkRes,
                                        in memoryRequirements,
                                        isDynamicBuffer ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit,
                                        true);

        VkRes.Vk.BindBufferMemory(VkRes.GraphicsDevice.Handle, buffer, deviceMemory.Handle, 0).ThrowCode();

        BufferDeviceAddressInfo bufferDeviceAddressInfo = new()
        {
            SType = StructureType.BufferDeviceAddressInfo,
            Buffer = buffer
        };

        ulong address = VkRes.Vk.GetBufferDeviceAddress(VkRes.GraphicsDevice.Handle, &bufferDeviceAddressInfo);

        Handle = buffer;
        DeviceMemory = deviceMemory;
        Address = address;
        SizeInBytes = sizeInBytes;
        Usage = BufferUsage.Internal;
        IsHostVisible = isDynamicBuffer;
    }

    internal override VkBuffer Handle { get; }

    internal DeviceMemory DeviceMemory { get; }

    internal ulong Address { get; }

    public uint SizeInBytes { get; }

    public BufferUsage Usage { get; }

    public bool IsHostVisible { get; }

    public void* Map(ulong sizeInBytes, ulong offsetInBytes = 0)
    {
        if (!IsHostVisible)
        {
            throw new InvalidOperationException("Cannot map a device buffer that is not host visible.");
        }

        void* data;
        VkRes.Vk.MapMemory(VkRes.GraphicsDevice.Handle, DeviceMemory.Handle, offsetInBytes, sizeInBytes, 0, &data).ThrowCode();

        return data;
    }

    public void Unmap()
    {
        if (IsHostVisible)
        {
            VkRes.Vk.UnmapMemory(VkRes.GraphicsDevice.Handle, DeviceMemory.Handle);
        }
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        VkRes.Vk.DestroyBuffer(VkRes.GraphicsDevice.Handle, Handle, null);

        DeviceMemory.Dispose();
    }
}
