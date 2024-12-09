using ZenithEngine.Common;

namespace ZenithEngine.Vulkan;

internal class VKQueueAllocator : DisposableObject
{
    private readonly Lock @lock = new();
    private readonly Queue<VkQueue> available = [];
    private readonly List<VkQueue> inUse = [];

    public VKQueueAllocator(VKGraphicsContext context,
                            uint queueFamilyIndex,
                            uint queueCount)
    {
        for (uint i = 0; i < queueCount; i++)
        {
            available.Enqueue(context.Vk.GetDeviceQueue(context.Device, queueFamilyIndex, i));
        }
    }

    public VkQueue Alloc()
    {
        using Lock.Scope _ = @lock.EnterScope();

        if (available.Count is 0)
        {
            throw new InvalidOperationException("No available queues.");
        }

        VkQueue queue = available.Dequeue();

        inUse.Add(queue);

        return queue;
    }

    public void Free(VkQueue queue)
    {
        using Lock.Scope _ = @lock.EnterScope();

        if (inUse.Remove(queue))
        {
            available.Enqueue(queue);
        }
    }

    protected override void Destroy()
    {
        inUse.Clear();
        available.Clear();
    }
}
