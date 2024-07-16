using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class DeviceBuffer : DeviceResource, IBindableResource
{
    private readonly VkBuffer _buffer;
    private readonly DeviceMemory _deviceMemory;
    private readonly uint _sizeInBytes;
    private readonly bool _isHostVisible;

    internal DeviceBuffer(GraphicsDevice graphicsDevice, ref readonly BufferDescription description) : base(graphicsDevice)
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

        BufferCreateInfo createInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = description.SizeInBytes,
            Usage = bufferUsageFlags,
            SharingMode = SharingMode.Exclusive
        };

        VkBuffer buffer;
        Vk.CreateBuffer(Device, &createInfo, null, &buffer).ThrowCode();

        MemoryRequirements memoryRequirements;
        Vk.GetBufferMemoryRequirements(Device, buffer, &memoryRequirements);

        bool isStaging = description.Usage.HasFlag(BufferUsage.Staging);
        bool hostVisible = isStaging || description.Usage.HasFlag(BufferUsage.Dynamic);

        DeviceMemory deviceMemory = new(graphicsDevice,
                                        in memoryRequirements,
                                        hostVisible ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit);

        Vk.BindBufferMemory(Device, buffer, deviceMemory.Handle, 0).ThrowCode();

        _buffer = buffer;
        _deviceMemory = deviceMemory;
        _sizeInBytes = description.SizeInBytes;
        _isHostVisible = hostVisible;
    }

    internal VkBuffer Handle => _buffer;

    internal DeviceMemory DeviceMemory => _deviceMemory;

    public uint SizeInBytes => _sizeInBytes;

    public bool IsHostVisible => _isHostVisible;

    public void* Map(ulong sizeInBytes, ulong offsetInBytes = 0)
    {
        if (!IsHostVisible)
        {
            throw new InvalidOperationException("Cannot map a device buffer that is not host visible.");
        }

        void* data;
        Vk.MapMemory(Device, _deviceMemory.Handle, offsetInBytes, sizeInBytes, 0, &data).ThrowCode();

        return data;
    }

    public void Unmap()
    {
        if (IsHostVisible)
        {
            Vk.UnmapMemory(Device, _deviceMemory.Handle);
        }
    }

    protected override void Destroy()
    {
        Vk.DestroyBuffer(Device, _buffer, null);

        _deviceMemory.Dispose();
    }
}
