using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKDeviceMemory : VKDeviceResource
{
    public VKDeviceMemory(VKContext context, MemoryRequirements requirements, MemoryPropertyFlags properties) : base(context)
    {
        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = context.FindMemoryTypeIndex(requirements.MemoryTypeBits, properties)
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
