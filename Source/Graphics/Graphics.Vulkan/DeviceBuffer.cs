using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class DeviceBuffer : DeviceResource, IBindableResource
{
    private readonly VkBuffer _buffer;
    private readonly ulong _address;
    private readonly DeviceMemory _deviceMemory;
    private readonly uint _sizeInBytes;
    private readonly BufferUsage _usage;
    private readonly bool _isHostVisible;

    internal DeviceBuffer(GraphicsDevice graphicsDevice,
                          ref readonly BufferDescription description,
                          BufferUsageFlags otherFlags = BufferUsageFlags.None) : base(graphicsDevice)
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
            bufferUsageFlags |= BufferUsageFlags.ShaderDeviceAddressBit;
        }

        if (description.Usage.HasFlag(BufferUsage.StructuredBufferReadOnly)
            || description.Usage.HasFlag(BufferUsage.StructuredBufferReadWrite))
        {
            bufferUsageFlags |= BufferUsageFlags.StorageBufferBit;
            bufferUsageFlags |= BufferUsageFlags.ShaderDeviceAddressBit;
        }

        if (description.Usage.HasFlag(BufferUsage.IndirectBuffer))
        {
            bufferUsageFlags |= BufferUsageFlags.IndirectBufferBit;
        }

        bufferUsageFlags |= otherFlags;

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
                                        hostVisible ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit, bufferUsageFlags.HasFlag(BufferUsageFlags.ShaderDeviceAddressBit));

        Vk.BindBufferMemory(Device, buffer, deviceMemory.Handle, 0).ThrowCode();

        ulong address = 0;
        if (bufferUsageFlags.HasFlag(BufferUsageFlags.ShaderDeviceAddressBit))
        {
            BufferDeviceAddressInfo bufferDeviceAddressInfo = new()
            {
                SType = StructureType.BufferDeviceAddressInfo,
                Buffer = buffer
            };

            address = Vk.GetBufferDeviceAddress(Device, &bufferDeviceAddressInfo);
        }

        _buffer = buffer;
        _address = address;
        _deviceMemory = deviceMemory;
        _sizeInBytes = description.SizeInBytes;
        _usage = description.Usage;
        _isHostVisible = hostVisible;
    }

    internal VkBuffer Handle => _buffer;

    internal ulong Address => _address;

    internal DeviceMemory DeviceMemory => _deviceMemory;

    public uint SizeInBytes => _sizeInBytes;

    public BufferUsage Usage => _usage;

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
