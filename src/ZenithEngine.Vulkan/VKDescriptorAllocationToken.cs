namespace ZenithEngine.Vulkan;

internal readonly struct VKDescriptorAllocationToken(VKDescriptorPool pool, VkDescriptorSet set)
{
    public readonly VKDescriptorPool Pool = pool;

    public readonly VkDescriptorSet Set = set;
}
