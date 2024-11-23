using Silk.NET.Vulkan;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDeviceMemory : GraphicsResource
{
    public VkDeviceMemory DeviceMemory;

    public VKDeviceMemory(GraphicsContext context,
                          MemoryRequirements requirements,
                          bool isDynamic) : base(context)
    {
        MemoryPropertyFlags flags = isDynamic
            ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
            : MemoryPropertyFlags.DeviceLocalBit;

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = Context.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
        };

        allocateInfo.AddNext(out MemoryAllocateFlagsInfo flagsInfo);

        flagsInfo.Flags = MemoryAllocateFlags.AddressBit;

        Context.Vk.AllocateMemory(Context.Device,
                                  &allocateInfo,
                                  null,
                                  out DeviceMemory).ThrowIfError();
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.DeviceMemory, DeviceMemory.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.FreeMemory(Context.Device, DeviceMemory, null);
    }
}
