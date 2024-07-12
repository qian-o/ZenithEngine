using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class DeviceMemory : DeviceResource
{
    private readonly VkDeviceMemory _deviceMemory;

    internal DeviceMemory(GraphicsDevice graphicsDevice, ref readonly MemoryRequirements requirements, MemoryPropertyFlags flags) : base(graphicsDevice)
    {
        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = PhysicalDevice.FindMemoryTypeIndex(requirements.MemoryTypeBits, flags)
        };

        VkDeviceMemory deviceMemory;
        Vk.AllocateMemory(Device, &allocateInfo, null, &deviceMemory).ThrowCode();

        _deviceMemory = deviceMemory;
    }

    internal VkDeviceMemory Handle => _deviceMemory;

    public void* Map(ulong size, ulong offset = 0)
    {
        void* data;
        Vk.MapMemory(Device, _deviceMemory, offset, size, 0, &data).ThrowCode();

        return data;
    }

    public void Unmap()
    {
        Vk.UnmapMemory(Device, _deviceMemory);
    }

    protected override void Destroy()
    {
        Vk.FreeMemory(Device, _deviceMemory, null);
    }
}
