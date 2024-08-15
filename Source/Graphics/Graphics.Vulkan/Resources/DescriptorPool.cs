using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class DescriptorPool : DeviceResource
{
    private readonly VkDescriptorPool _descriptorPool;

    private uint remainingSets;
    private uint uniformBufferCount;
    private uint uniformBufferDynamicCount;
    private uint sampledImageCount;
    private uint samplerCount;
    private uint storageBufferCount;
    private uint storageBufferDynamicCount;
    private uint storageImageCount;

    internal DescriptorPool(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
        const uint maxSets = 1000;
        const uint descriptorCount = 100;
        const uint poolSizeCount = 7;

        DescriptorPoolSize[] sizes = new DescriptorPoolSize[poolSizeCount];
        sizes[0] = new DescriptorPoolSize
        {
            Type = DescriptorType.UniformBuffer,
            DescriptorCount = descriptorCount
        };
        sizes[1] = new DescriptorPoolSize
        {
            Type = DescriptorType.SampledImage,
            DescriptorCount = descriptorCount
        };
        sizes[2] = new DescriptorPoolSize
        {
            Type = DescriptorType.Sampler,
            DescriptorCount = descriptorCount
        };
        sizes[3] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageBuffer,
            DescriptorCount = descriptorCount
        };
        sizes[4] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageImage,
            DescriptorCount = descriptorCount
        };
        sizes[5] = new DescriptorPoolSize
        {
            Type = DescriptorType.UniformBufferDynamic,
            DescriptorCount = descriptorCount
        };
        sizes[6] = new DescriptorPoolSize
        {
            Type = DescriptorType.StorageBufferDynamic,
            DescriptorCount = descriptorCount
        };

        DescriptorPoolCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = maxSets,
            PoolSizeCount = poolSizeCount,
            PPoolSizes = sizes.AsPointer(),
            Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
        };

        VkDescriptorPool descriptorPool;
        Vk.CreateDescriptorPool(Device, &createInfo, null, &descriptorPool).ThrowCode();

        _descriptorPool = descriptorPool;

        remainingSets = maxSets;
        uniformBufferCount = descriptorCount;
        uniformBufferDynamicCount = descriptorCount;
        sampledImageCount = descriptorCount;
        samplerCount = descriptorCount;
        storageBufferCount = descriptorCount;
        storageBufferDynamicCount = descriptorCount;
        storageImageCount = descriptorCount;
    }

    internal VkDescriptorPool Handle => _descriptorPool;

    public bool TryAllocate(ResourceLayout layout, out DescriptorAllocationToken token)
    {
        VkDescriptorSetLayout setLayout = layout.Handle;
        DescriptorResourceCounts counts = layout.Counts;

        if (remainingSets > 0
            && uniformBufferCount >= counts.UniformBufferCount
            && uniformBufferDynamicCount >= counts.UniformBufferDynamicCount
            && sampledImageCount >= counts.SampledImageCount
            && samplerCount >= counts.SamplerCount
            && storageBufferCount >= counts.StorageBufferCount
            && storageBufferDynamicCount >= counts.StorageBufferDynamicCount
            && storageImageCount >= counts.StorageImageCount)
        {
            remainingSets--;
            uniformBufferCount -= counts.UniformBufferCount;
            uniformBufferDynamicCount -= counts.UniformBufferDynamicCount;
            sampledImageCount -= counts.SampledImageCount;
            samplerCount -= counts.SamplerCount;
            storageBufferCount -= counts.StorageBufferCount;
            storageBufferDynamicCount -= counts.StorageBufferDynamicCount;
            storageImageCount -= counts.StorageImageCount;

            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = _descriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = &setLayout
            };

            VkDescriptorSet descriptorSet;
            Vk.AllocateDescriptorSets(Device, &allocateInfo, &descriptorSet).ThrowCode();

            token = new DescriptorAllocationToken(_descriptorPool, descriptorSet, counts);

            return true;
        }
        else
        {
            token = default;

            return false;
        }
    }

    public DescriptorAllocationToken Allocate(ResourceLayout layout)
    {
        if (TryAllocate(layout, out DescriptorAllocationToken token))
        {
            return token;
        }
        else
        {
            throw new InvalidOperationException("Failed to allocate descriptor set.");
        }
    }

    public bool Free(DescriptorAllocationToken token)
    {
        if (token.Pool.Handle == _descriptorPool.Handle)
        {
            Vk.FreeDescriptorSets(Device, _descriptorPool, 1, &token.Set).ThrowCode();

            remainingSets++;
            uniformBufferCount += token.Counts.UniformBufferCount;
            uniformBufferDynamicCount += token.Counts.UniformBufferDynamicCount;
            sampledImageCount += token.Counts.SampledImageCount;
            samplerCount += token.Counts.SamplerCount;
            storageBufferCount += token.Counts.StorageBufferCount;
            storageBufferDynamicCount += token.Counts.StorageBufferDynamicCount;
            storageImageCount += token.Counts.StorageImageCount;

            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void Destroy()
    {
        Vk.DestroyDescriptorPool(Device, _descriptorPool, null);
    }
}
