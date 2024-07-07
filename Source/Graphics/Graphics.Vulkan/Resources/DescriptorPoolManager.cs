namespace Graphics.Vulkan;

internal sealed class DescriptorPoolManager(GraphicsDevice graphicsDevice) : DeviceResource(graphicsDevice)
{
    private readonly List<DescriptorPool> pools = [];
    private readonly object _locker = new();

    public DescriptorAllocationToken Allocate(ResourceLayout layout)
    {
        lock (_locker)
        {
            foreach (DescriptorPool pool in pools)
            {
                if (pool.TryAllocate(layout, out DescriptorAllocationToken token))
                {
                    return token;
                }
            }

            DescriptorPool newPool = new(GraphicsDevice);
            pools.Add(newPool);

            return newPool.Allocate(layout);
        }
    }

    public void Free(DescriptorAllocationToken token)
    {
        lock (_locker)
        {
            foreach (DescriptorPool pool in pools)
            {
                if (pool.Free(token))
                {
                    return;
                }
            }

            throw new InvalidOperationException("Failed to free descriptor token");
        }
    }

    protected override void Destroy()
    {
        foreach (DescriptorPool pool in pools)
        {
            pool.Dispose();
        }

        pools.Clear();
    }
}
