using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class Semaphore : DeviceResource
{
    private readonly VkSemaphore _semaphore;

    public Semaphore(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
        SemaphoreCreateInfo createInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        VkSemaphore semaphore;
        Vk.CreateSemaphore(Device, &createInfo, null, &semaphore).ThrowCode();

        _semaphore = semaphore;
    }

    public VkSemaphore Handle => _semaphore;

    protected override void Destroy()
    {
        Vk.DestroySemaphore(Device, _semaphore, null);
    }
}
