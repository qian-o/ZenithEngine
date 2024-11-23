namespace ZenithEngine.Vulkan;

internal readonly struct VKResourceCounts(uint uniformBufferCount,
                                          uint storageBufferCount,
                                          uint sampledImageCount,
                                          uint storageImageCount,
                                          uint samplerCount,
                                          uint accelerationStructureCount)
{
    public uint UniformBufferCount { get; } = uniformBufferCount;

    public uint StorageBufferCount { get; } = storageBufferCount;

    public uint SampledImageCount { get; } = sampledImageCount;

    public uint StorageImageCount { get; } = storageImageCount;

    public uint SamplerCount { get; } = samplerCount;

    public uint AccelerationStructureCount { get; } = accelerationStructureCount;
}
