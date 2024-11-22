using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKBuffer : Buffer
{
    public VkBuffer Buffer;

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

        Context.Vk.CreateBuffer(Context.Device, &createInfo, null, out Buffer).ThrowIfError();
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.Buffer, Buffer.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyBuffer(Context.Device, Buffer, null);
    }
}
