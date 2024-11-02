using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKBuffer : Buffer
{
    private readonly VKContext vkContext;

    public VKBuffer(Context context, ref readonly BufferDescription description) : base(context, in description)
    {
        vkContext = (VKContext)context;

        BufferCreateInfo createInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = description.SizeInBytes,
            Usage = BufferUsageFlags.TransferSrcBit
                    | BufferUsageFlags.TransferDstBit
                    | BufferUsageFlags.ShaderDeviceAddressBit,
            SharingMode = SharingMode.Exclusive
        };

        if (description.Usage.HasFlag(BufferUsage.VertexBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.VertexBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.IndexBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.IndexBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.ConstantBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.UniformBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.StorageBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.StorageBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.IndirectBuffer))
        {
            createInfo.Usage |= BufferUsageFlags.IndirectBufferBit;
        }

        if (description.Usage.HasFlag(BufferUsage.AccelerationStructure))
        {
            createInfo.Usage |= BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr;
        }

        VkBuffer buffer;
        vkContext.Vk.CreateBuffer(vkContext.Device, &createInfo, null, &buffer).ThrowCode();

        MemoryRequirements memoryRequirements;
        vkContext.Vk.GetBufferMemoryRequirements(vkContext.Device, buffer, &memoryRequirements);

        bool isHostVisible = description.Usage.HasFlag(BufferUsage.Dynamic);

        DeviceMemory = new(vkContext,
                           memoryRequirements,
                           isHostVisible ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit);

        vkContext.Vk.BindBufferMemory(vkContext.Device, buffer, DeviceMemory.DeviceMemory, 0).ThrowCode();

        BufferDeviceAddressInfo bufferDeviceAddressInfo = new()
        {
            SType = StructureType.BufferDeviceAddressInfo,
            Buffer = buffer
        };

        Address = vkContext.Vk.GetBufferDeviceAddress(vkContext.Device, &bufferDeviceAddressInfo);

        Buffer = buffer;
    }

    public VkBuffer Buffer { get; }

    public VKDeviceMemory DeviceMemory { get; }

    public ulong Address { get; }

    protected override void SetName(string name)
    {
        vkContext.SetDebugName(ObjectType.Buffer, Buffer.Handle, name);
    }

    protected override void Destroy()
    {
        DeviceMemory.Dispose();

        vkContext.Vk.DestroyBuffer(vkContext.Device, Buffer, null);
    }
}
