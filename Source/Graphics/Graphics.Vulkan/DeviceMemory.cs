using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class DeviceMemory : DeviceResource
{
    private readonly VkDeviceMemory _deviceMemory;

    internal DeviceMemory(GraphicsDevice graphicsDevice, ref readonly MemoryRequirements requirements, MemoryPropertyFlags flags, bool isAddress) : base(graphicsDevice)
    {
        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = PhysicalDevice.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
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
        Vk.AllocateMemory(Device, &allocateInfo, null, &deviceMemory).ThrowCode();

        _deviceMemory = deviceMemory;
    }

    internal VkDeviceMemory Handle => _deviceMemory;

    protected override void Destroy()
    {
        Vk.FreeMemory(Device, _deviceMemory, null);
    }
}
