namespace Graphics.Vulkan;

internal readonly struct DescriptorAllocationToken(VkDescriptorPool pool,
                                                   VkDescriptorSet set,
                                                   DescriptorResourceCounts counts)
{
    public readonly VkDescriptorPool Pool = pool;

    public readonly VkDescriptorSet Set = set;

    public readonly DescriptorResourceCounts Counts = counts;
}
