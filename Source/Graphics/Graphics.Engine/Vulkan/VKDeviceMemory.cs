using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKDeviceMemory : VKDeviceResource
{
    public VKDeviceMemory(VKContext context,
                          bool isHostVisible,
                          MemoryRequirements requirements,
                          MemoryPropertyFlags flags = MemoryPropertyFlags.None) : base(context)
    {
        if (isHostVisible)
        {
            flags |= MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
        }
        else
        {
            flags |= MemoryPropertyFlags.DeviceLocalBit;
        }

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = context.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
        };

        allocateInfo.AddNext(out MemoryAllocateFlagsInfo memoryAllocateFlagsInfo);

        memoryAllocateFlagsInfo.Flags = MemoryAllocateFlags.AddressBit;

        VkDeviceMemory deviceMemory;
        context.Vk.AllocateMemory(context.Device, &allocateInfo, null, &deviceMemory).ThrowCode();

        DeviceMemory = deviceMemory;
    }

    public VkDeviceMemory DeviceMemory { get; }

    protected override void Destroy()
    {
        Context.Vk.FreeMemory(Context.Device, DeviceMemory, null);
    }
}
