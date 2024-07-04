using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Buffer : DeviceResource
{
    private readonly VkBuffer _buffer;
    private readonly DeviceMemory _deviceMemory;

    internal Buffer(GraphicsDevice graphicsDevice, ref readonly BufferDescription description) : base(graphicsDevice)
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
            bufferUsageFlags |= BufferUsageFlags.UniformBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.StructuredBufferReadOnly)
            || description.Usage.HasFlag(BufferUsage.StructuredBufferReadWrite))
        {
            bufferUsageFlags |= BufferUsageFlags.StorageBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.IndirectBuffer))
        {
            bufferUsageFlags |= BufferUsageFlags.IndirectBufferBit;
        }

        BufferCreateInfo bufferCreateInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = description.SizeInBytes,
            Usage = bufferUsageFlags,
            SharingMode = SharingMode.Exclusive
        };

        VkBuffer buffer;
        Vk.CreateBuffer(Device, &bufferCreateInfo, null, &buffer);

        MemoryRequirements memoryRequirements;
        Vk.GetBufferMemoryRequirements(Device, buffer, &memoryRequirements);

        bool isStaging = description.Usage.HasFlag(BufferUsage.Staging);
        bool hostVisible = isStaging || description.Usage.HasFlag(BufferUsage.Dynamic);

        DeviceMemory deviceMemory = new(graphicsDevice,
                                        in memoryRequirements,
                                        hostVisible ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit);

        Vk.BindBufferMemory(Device, buffer, deviceMemory.Handle, 0);

        _buffer = buffer;
        _deviceMemory = deviceMemory;
    }

    internal VkBuffer Handle => _buffer;

    protected override void Destroy()
    {
        Vk.DestroyBuffer(Device, _buffer, null);

        _deviceMemory.Dispose();
    }
}
