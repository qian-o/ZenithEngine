using Silk.NET.Vulkan;
using ZenithEngine.Common;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDescriptorSetAllocator(VKGraphicsContext context) : DisposableObject
{
    private readonly Lock @lock = new();
    private readonly List<VKDescriptorPool> pools = [];

    public VKDescriptorAllocationToken Alloc(VkDescriptorSetLayout descriptorSetLayout,
                                             VKResourceCounts counts)
    {
        VKDescriptorPool pool = GetPool(counts);

        DescriptorSetAllocateInfo allocateInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = pool.Pool,
            DescriptorSetCount = 1,
            PSetLayouts = &descriptorSetLayout
        };

        VkDescriptorSet set;
        context.Vk.AllocateDescriptorSets(context.Device, &allocateInfo, &set).ThrowIfError();

        return new VKDescriptorAllocationToken(pool, set);
    }

    public void Free(VKDescriptorAllocationToken token)
    {
        context.Vk.FreeDescriptorSets(context.Device,
                                      token.Pool.Pool,
                                      1,
                                      &token.Set).ThrowIfError();
    }

    protected override void Destroy()
    {
        foreach (VKDescriptorPool pool in pools)
        {
            pool.Dispose();
        }

        pools.Clear();
    }

    private VKDescriptorPool GetPool(VKResourceCounts counts)
    {
        @lock.Enter();

        VKDescriptorPool? pool = pools.FirstOrDefault(p => p.CanAlloc(counts));

        if (pool is null)
        {
            pool = new(context);

            pools.Add(pool);
        }

        @lock.Exit();

        return pool;
    }
}
