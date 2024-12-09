using ZenithEngine.Common;

namespace ZenithEngine.Vulkan;

internal unsafe class VKQueueAllocator : DisposableObject
{
    private readonly Queue<VkQueue> available = new();

    public VKQueueAllocator(VKGraphicsContext context,
                            uint queueFamilyIndex,
                            uint queueCount)
    {
        for (uint i = 0; i < queueCount; i++)
        {
            available.Enqueue(context.Vk.GetDeviceQueue(context.Device, queueFamilyIndex, i));
        }
    }

    public VkQueue Allocate()
    {
        return available.Dequeue();
    }

    public void Free(VkQueue queue)
    {
        available.Enqueue(queue);
    }

    protected override void Destroy()
    {

    }
}
