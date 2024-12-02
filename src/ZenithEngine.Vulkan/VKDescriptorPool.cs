using Silk.NET.Vulkan;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDescriptorPool : GraphicsResource
{
    private const uint MaxSets = 100;
    private const uint DescriptorCount = 1000;

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
        bool supportAS = Context.Capabilities.IsRayTracingSupported
                         || Context.Capabilities.IsRayQuerySupported;

        uint sizeCount = supportAS ? 8u : 7u;
        DescriptorPoolSize* sizes = Allocator.Alloc<DescriptorPoolSize>(sizeCount);

        sizes[0] = new()
        {
            Type = DescriptorType.UniformBuffer,
            DescriptorCount = DescriptorCount
        };

        sizes[1] = new()
        {
            Type = DescriptorType.UniformBufferDynamic,
            DescriptorCount = DescriptorCount
        };

        sizes[2] = new()
        {
            Type = DescriptorType.StorageBuffer,
            DescriptorCount = DescriptorCount
        };

        sizes[3] = new()
        {
            Type = DescriptorType.StorageBufferDynamic,
            DescriptorCount = DescriptorCount
        };

        sizes[4] = new()
        {
            Type = DescriptorType.SampledImage,
            DescriptorCount = DescriptorCount
        };

        sizes[5] = new()
        {
            Type = DescriptorType.StorageImage,
            DescriptorCount = DescriptorCount
        };

        sizes[6] = new()
        {
            Type = DescriptorType.Sampler,
            DescriptorCount = DescriptorCount
        };

        if (supportAS)
        {
            sizes[7] = new()
            {
                Type = DescriptorType.AccelerationStructureKhr,
                DescriptorCount = DescriptorCount
            };
        }

        DescriptorPoolCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = MaxSets,
            PoolSizeCount = sizeCount,
            PPoolSizes = sizes,
            Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
        };

        Context.Vk.CreateDescriptorPool(Context.Device,
                                        &createInfo,
                                        null,
                                        out Pool).ThrowIfError();

        Allocator.Free(sizes);
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

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
