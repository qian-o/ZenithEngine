using Silk.NET.Vulkan;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDescriptorPool : GraphicsResource
{
    private const uint MaxSets = 1000;
    private const uint DescriptorCount = 100;

    public VkDescriptorPool Pool;

    private uint remainingSets = MaxSets;
    private uint uniformBufferCount = DescriptorCount;
    private uint storageBufferCount = DescriptorCount;
    private uint sampledImageCount = DescriptorCount;
    private uint storageImageCount = DescriptorCount;
    private uint samplerCount = DescriptorCount;
    private uint accelerationStructureCount = DescriptorCount;

    public VKDescriptorPool(GraphicsContext context) : base(context)
    {
        DescriptorPoolSize* poolSizes = MemoryAllocator.Alloc<DescriptorPoolSize>(8);

        poolSizes[0] = new DescriptorPoolSize
        {
            Type = DescriptorType.UniformBuffer,
            DescriptorCount = DescriptorCount
        };

        poolSizes[1] = new DescriptorPoolSize
        {
            Type = DescriptorType.UniformBufferDynamic,
            DescriptorCount = DescriptorCount
        };

        poolSizes[2] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageBuffer,
            DescriptorCount = DescriptorCount
        };

        poolSizes[3] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageBufferDynamic,
            DescriptorCount = DescriptorCount
        };

        poolSizes[4] = new DescriptorPoolSize
        {
            Type = DescriptorType.SampledImage,
            DescriptorCount = DescriptorCount
        };

        poolSizes[5] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageImage,
            DescriptorCount = DescriptorCount
        };

        poolSizes[6] = new DescriptorPoolSize
        {
            Type = DescriptorType.Sampler,
            DescriptorCount = DescriptorCount
        };

        poolSizes[7] = new DescriptorPoolSize
        {
            Type = DescriptorType.AccelerationStructureKhr,
            DescriptorCount = DescriptorCount
        };

        DescriptorPoolCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = MaxSets,
            PoolSizeCount = 8,
            PPoolSizes = poolSizes,
            Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
        };

        Context.Vk.CreateDescriptorPool(Context.Device,
                                        &createInfo,
                                        null,
                                        out Pool).ThrowIfError();

        MemoryAllocator.Free(poolSizes);
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public bool CanAlloc(VKResourceCounts counts)
    {
        if (remainingSets < 1 ||
            uniformBufferCount < counts.UniformBufferCount ||
            storageBufferCount < counts.StorageBufferCount ||
            sampledImageCount < counts.SampledImageCount ||
            storageImageCount < counts.StorageImageCount ||
            samplerCount < counts.SamplerCount ||
            accelerationStructureCount < counts.AccelerationStructureCount)
        {
            return false;
        }

        remainingSets--;
        uniformBufferCount -= counts.UniformBufferCount;
        storageBufferCount -= counts.StorageBufferCount;
        sampledImageCount -= counts.SampledImageCount;
        storageImageCount -= counts.StorageImageCount;
        samplerCount -= counts.SamplerCount;
        accelerationStructureCount -= counts.AccelerationStructureCount;

        return true;
    }

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorPool, Pool.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyDescriptorPool(Context.Device, Pool, null);
    }
}
