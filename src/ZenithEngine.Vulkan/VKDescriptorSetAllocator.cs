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
        using Lock.Scope _ = @lock.EnterScope();

        if (pools.FirstOrDefault(item => item.CanAlloc(counts)) is not VKDescriptorPool pool)
        {
            pools.Add(pool = new(context));
        }

        DescriptorSetAllocateInfo allocateInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = pool.Pool,
            DescriptorSetCount = 1,
            PSetLayouts = &descriptorSetLayout
        };

        VkDescriptorSet set;
        context.Vk.AllocateDescriptorSets(context.Device,
                                          &allocateInfo,
                                          &set).ThrowIfError();

        return new(pool, set);
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
}
