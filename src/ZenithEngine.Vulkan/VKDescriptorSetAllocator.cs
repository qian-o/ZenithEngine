using Silk.NET.Vulkan;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDescriptorSetAllocator(GraphicsContext context) : GraphicsResource(context)
{
    private readonly Lock @lock = new();
    private readonly List<VKDescriptorPool> pools = [];

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public VKDescriptorAllocationToken Alloc(VKResourceLayout layout)
    {
        using Lock.Scope _ = @lock.EnterScope();

        if (pools.FirstOrDefault(item => item.CanAlloc(layout.Counts)) is not VKDescriptorPool pool)
        {
            pools.Add(pool = new(Context));
        }

        fixed (DescriptorSetLayout* descriptorSetLayout = &layout.DescriptorSetLayout)
        {
            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = pool.Pool,
                DescriptorSetCount = 1,
                PSetLayouts = descriptorSetLayout
            };

            VkDescriptorSet set;
            Context.Vk.AllocateDescriptorSets(Context.Device,
                                              &allocateInfo,
                                              &set).ThrowIfError();

            return new(pool, set);
        }
    }

    public void Free(VKDescriptorAllocationToken token)
    {
        Context.Vk.FreeDescriptorSets(Context.Device,
                                      token.Pool.Pool,
                                      1,
                                      &token.Set).ThrowIfError();
    }

    protected override void DebugName(string name)
    {
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
