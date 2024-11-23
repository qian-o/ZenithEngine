namespace ZenithEngine.Vulkan;

internal readonly struct VKResourceCounts(uint uniformBufferCount,
                                          uint storageBufferCount,
                                          uint sampledImageCount,
                                          uint storageImageCount,
                                          uint samplerCount,
                                          uint accelerationStructureCount)
{
    public readonly uint UniformBufferCount = uniformBufferCount;

    public readonly uint StorageBufferCount = storageBufferCount;

    public readonly uint SampledImageCount = sampledImageCount;

    public readonly uint StorageImageCount = storageImageCount;

    public readonly uint SamplerCount = samplerCount;

    public readonly uint AccelerationStructureCount = accelerationStructureCount;
}
