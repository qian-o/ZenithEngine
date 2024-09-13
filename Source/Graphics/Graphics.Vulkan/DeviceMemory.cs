using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class DeviceMemory : VulkanObject<VkDeviceMemory>
{
    internal DeviceMemory(VulkanResources vkRes,
                          ref readonly MemoryRequirements requirements,
                          MemoryPropertyFlags flags,
                          bool isAddress) : base(vkRes, ObjectType.DeviceMemory)
    {
        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = VkRes.PhysicalDevice.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
        };

        if (isAddress)
        {
            MemoryAllocateFlagsInfoKHR memoryAllocateFlagsInfoKHR = new()
            {
                SType = StructureType.MemoryAllocateFlagsInfo,
                Flags = MemoryAllocateFlags.AddressBit,
            };

            allocateInfo.PNext = &memoryAllocateFlagsInfoKHR;
        }

        VkDeviceMemory deviceMemory;
        VkRes.Vk.AllocateMemory(VkRes.GraphicsDevice.Handle, &allocateInfo, null, &deviceMemory).ThrowCode();

        Handle = deviceMemory;
    }

    internal override VkDeviceMemory Handle { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    protected override void Destroy()
    {
        VkRes.Vk.FreeMemory(VkRes.GraphicsDevice.Handle, Handle, null);
    }
}
