using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceSet : DeviceResource
{
    private readonly DescriptorAllocationToken _token;

    internal ResourceSet(GraphicsDevice graphicsDevice, ref readonly ResourceSetDescription description) : base(graphicsDevice)
    {
        DescriptorAllocationToken token = DescriptorPoolManager.Allocate(description.Layout);

        DescriptorBufferInfo[] bufferInfos = new DescriptorBufferInfo[description.BoundResources.Length];
        DescriptorImageInfo[] imageInfos = new DescriptorImageInfo[description.BoundResources.Length];
        VkWriteDescriptorSet[] sets = new VkWriteDescriptorSet[description.BoundResources.Length];

        for (uint i = 0; i < description.BoundResources.Length; i++)
        {
            DescriptorType type = description.Layout.DescriptorTypes[i];

            WriteDescriptorSet set = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DescriptorCount = 1,
                DescriptorType = type,
                DstBinding = i,
                DstSet = token.Set
            };

            if (type == DescriptorType.UniformBuffer
                || type == DescriptorType.UniformBufferDynamic
                || type == DescriptorType.StorageBuffer
                || type == DescriptorType.StorageBufferDynamic)
            {
                DeviceBufferRange range = Util.GetBufferRange(description.BoundResources[i], 0);

                bufferInfos[i] = new DescriptorBufferInfo
                {
                    Buffer = range.Buffer.Handle,
                    Offset = range.Offset,
                    Range = range.SizeInBytes
                };

                set.PBufferInfo = (DescriptorBufferInfo*)Unsafe.AsPointer(ref bufferInfos[i]);
            }
            else if (type == DescriptorType.SampledImage)
            {
                TextureView textureView = (TextureView)description.BoundResources[i];

                imageInfos[i] = new DescriptorImageInfo
                {
                    ImageView = textureView.Handle,
                    ImageLayout = ImageLayout.ShaderReadOnlyOptimal
                };

                set.PImageInfo = (DescriptorImageInfo*)Unsafe.AsPointer(ref imageInfos[i]);
            }
            else if (type == DescriptorType.StorageImage)
            {
                TextureView textureView = (TextureView)description.BoundResources[i];

                imageInfos[i] = new DescriptorImageInfo
                {
                    ImageView = textureView.Handle,
                    ImageLayout = ImageLayout.General
                };

                set.PImageInfo = (DescriptorImageInfo*)Unsafe.AsPointer(ref imageInfos[i]);
            }
            else if (type == DescriptorType.Sampler)
            {
                Sampler sampler = (Sampler)description.BoundResources[i];

                imageInfos[i] = new DescriptorImageInfo
                {
                    Sampler = sampler.Handle
                };

                set.PImageInfo = (DescriptorImageInfo*)Unsafe.AsPointer(ref imageInfos[i]);
            }
            else
            {
                throw new NotSupportedException();
            }

            sets[i] = set;
        }

        Vk.UpdateDescriptorSets(Device,
                                (uint)sets.Length,
                                (VkWriteDescriptorSet*)Unsafe.AsPointer(ref sets[0]),
                                0,
                                null);

        _token = token;
    }

    public VkDescriptorSet Handle => _token.Set;

    protected override void Destroy()
    {
        DescriptorPoolManager.Free(_token);
    }
}
