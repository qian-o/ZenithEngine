using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKBuffer : Buffer
{
    public VKBuffer(Context context,
                    ref readonly BufferDescription description) : base(context, in description)
    {
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
        Context.Vk.CreateBuffer(Context.Device, &createInfo, null, &buffer).ThrowCode();

        MemoryRequirements memoryRequirements;
        Context.Vk.GetBufferMemoryRequirements(Context.Device, buffer, &memoryRequirements);

        DeviceMemory = new(Context, description.Usage.HasFlag(BufferUsage.Dynamic), memoryRequirements);

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

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Buffer, Buffer.Handle, name);
        Context.SetDebugName(ObjectType.DeviceMemory, DeviceMemory.DeviceMemory.Handle, $"{name} Memory");
    }

    protected override void Destroy()
    {
        DeviceMemory.Dispose();

        Context.Vk.DestroyBuffer(Context.Device, Buffer, null);
    }
}
