using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class DeviceMemory : DeviceResource
{
    private readonly VkDeviceMemory _deviceMemory;

    public DeviceMemory(GraphicsDevice graphicsDevice, in MemoryRequirements requirements, MemoryPropertyFlags flags) : base(graphicsDevice)
    {
        MemoryAllocateInfo memoryAllocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = PhysicalDevice.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
        };

        VkDeviceMemory deviceMemory;
        Vk.AllocateMemory(Device, in memoryAllocateInfo, null, &deviceMemory);

        _deviceMemory = deviceMemory;
    }

    protected override void Destroy()
    {
        Vk.FreeMemory(Device, _deviceMemory, null);
    }
}
