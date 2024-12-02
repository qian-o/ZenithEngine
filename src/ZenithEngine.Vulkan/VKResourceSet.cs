using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKResourceSet : ResourceSet
{
    public VKDescriptorAllocationToken Token;

    public VKResourceSet(GraphicsContext context,
                         ref readonly ResourceSetDesc desc) : base(context, in desc)
    {
        VKResourceLayout layout = desc.Layout.VK();

        Token = Context.DescriptorSetAllocator.Alloc(layout.DescriptorSetLayout, layout.Counts);

        WriteDescriptorSet[] writes = new WriteDescriptorSet[desc.Resources.Length];

        uint dynamicCount = 0;
        List<Texture> sampledImages = [];
        List<Texture> storageImages = [];

        uint resourceOffset = 0;

        for (uint i = 0; i < writes.Length; i++)
        {
            LayoutElementDesc element = layout.Desc.Elements[i];

            writes[i] = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = Token.Set,
                DstBinding = Utils.GetBinding(element.Type, element.Slot),
                DescriptorCount = element.Count,
                DescriptorType = VKFormats.GetDescriptorType(element.Type, element.Options)
            };

            FillDescriptors(MemoryAllocator,
                            element,
                            desc.Resources[(int)resourceOffset..(int)(resourceOffset + element.Count)],
                            ref writes[i],
                            ref dynamicCount,
                            sampledImages,
                            storageImages);

            resourceOffset += element.Count;
        }

        Context.Vk.UpdateDescriptorSets(Context.Device,
                                        (uint)writes.Length,
                                        writes,
                                        0,
                                        (CopyDescriptorSet*)null);

        MemoryAllocator.Release();

        DynamicCount = dynamicCount;
        SampledImages = [.. sampledImages];
        StorageImages = [.. storageImages];
    }

    public uint DynamicCount { get; }

    public Texture[] SampledImages { get; }

    public Texture[] StorageImages { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSet, Token.Set.Handle, name);
    }

    protected override void Destroy()
    {
        Array.Clear(StorageImages, 0, StorageImages.Length);
        Array.Clear(SampledImages, 0, SampledImages.Length);

        Context.DescriptorSetAllocator.Free(Token);
    }

    private static void FillDescriptors(MemoryAllocator allocator,
                                        LayoutElementDesc element,
                                        GraphicsResource[] resources,
                                        ref WriteDescriptorSet write,
                                        ref uint dynamicCount,
                                        List<Texture> sampledImages,
                                        List<Texture> storageImages)
    {
        if (write.DescriptorType
            is DescriptorType.UniformBuffer
            or DescriptorType.UniformBufferDynamic
            or DescriptorType.StorageBuffer
            or DescriptorType.StorageBufferDynamic)
        {
            DescriptorBufferInfo* infos = allocator.Alloc<DescriptorBufferInfo>(element.Count);

            for (uint i = 0; i < element.Count; i++)
            {
                Buffer buffer = (Buffer)resources[i];

                infos[i] = new()
                {
                    Buffer = buffer.VK().Buffer,
                    Offset = 0,
                    Range = element.Size == 0 ? Vk.WholeSize : element.Size
                };

                if (element.Options is ElementOptions.DynamicBinding)
                {
                    dynamicCount++;
                }
            }

            write.PBufferInfo = infos;
        }
        else if (write.DescriptorType is DescriptorType.SampledImage or DescriptorType.StorageImage)
        {
            DescriptorImageInfo* infos = allocator.Alloc<DescriptorImageInfo>(element.Count);

            bool isSampled = element.Type is ResourceType.Texture;
            ImageLayout imageLayout = isSampled ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General;

            for (uint i = 0; i < element.Count; i++)
            {
                TextureView textureView = (TextureView)resources[i];

                infos[i] = new()
                {
                    ImageView = textureView.VK().ImageView,
                    ImageLayout = imageLayout
                };

                if (isSampled)
                {
                    sampledImages.Add(textureView.Desc.Target);
                }
                else
                {
                    storageImages.Add(textureView.Desc.Target);
                }
            }

            write.PImageInfo = infos;
        }
        else if (write.DescriptorType is DescriptorType.Sampler)
        {
            DescriptorImageInfo* infos = allocator.Alloc<DescriptorImageInfo>(element.Count);

            for (uint i = 0; i < element.Count; i++)
            {
                Sampler sampler = (Sampler)resources[i];

                infos[i] = new()
                {
                    Sampler = sampler.VK().Sampler
                };
            }

            write.PImageInfo = infos;
        }
        else
        {
            throw new NotSupportedException("Resource type not supported.");
        }
    }
}
