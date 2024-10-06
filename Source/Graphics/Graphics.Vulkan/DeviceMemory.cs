using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class DeviceMemory : VulkanObject<VkDeviceMemory>
{
    internal DeviceMemory(VulkanResources vkRes,
                          ref readonly MemoryRequirements requirements,
                          MemoryPropertyFlags flags) : base(vkRes, ObjectType.DeviceMemory)
    {
        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = VkRes.PhysicalDevice.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
        };

        allocateInfo.AddNext(out MemoryAllocateFlagsInfoKHR memoryAllocateFlagsInfo);

        memoryAllocateFlagsInfo.Flags = MemoryAllocateFlags.AddressBit;

        VkDeviceMemory deviceMemory;
        VkRes.Vk.AllocateMemory(VkRes.VkDevice, &allocateInfo, null, &deviceMemory).ThrowCode();

        Handle = deviceMemory;
        SizeInBytes = requirements.Size;
    }

    internal override VkDeviceMemory Handle { get; }

    internal ulong SizeInBytes { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        VkRes.Vk.FreeMemory(VkRes.VkDevice, Handle, null);
    }
}
