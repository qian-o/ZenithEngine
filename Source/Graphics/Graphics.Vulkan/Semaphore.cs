using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class Semaphore : DeviceResource
{
    public Semaphore(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
        SemaphoreCreateInfo createInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        VkSemaphore semaphore;
        Vk.CreateSemaphore(Device, &createInfo, null, &semaphore).ThrowCode();

        Handle = semaphore;
    }

    public VkSemaphore Handle { get; }

    protected override void Destroy()
    {
        Vk.DestroySemaphore(Device, Handle, null);
    }
}
