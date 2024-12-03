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

        WriteDescriptorSet[] writes = new WriteDescriptorSet[layout.Desc.Elements.Length];

        uint resourceOffset = 0;
        List<Texture> sampledImages = [];
        List<Texture> storageImages = [];

        for (uint i = 0; i < writes.Length; i++)
        {
            LayoutElementDesc element = layout.Desc.Elements[i];

            writes[i] = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = Token.Set,
                DstBinding = Utils.GetBinding(element.Type, element.Slot),
                DescriptorCount = element.Count,
                DescriptorType = VKFormats.GetDescriptorType(element.Type, element.AllowDynamicOffset)
            };

            FillDescriptors(Allocator,
                            element,
                            desc.Resources[(int)resourceOffset..(int)(resourceOffset + element.Count)],
                            ref writes[i],
                            sampledImages,
                            storageImages);

            resourceOffset += element.Count;
        }

        Context.Vk.UpdateDescriptorSets(Context.Device,
                                        (uint)writes.Length,
                                        writes,
                                        0,
                                        (CopyDescriptorSet*)null);

        DynamicConstantBufferCount = layout.Desc.DynamicConstantBufferCount;
        SampledImages = [.. sampledImages];
        StorageImages = [.. storageImages];

        Allocator.Release();
    }

    public uint DynamicConstantBufferCount { get; }

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
                                        List<Texture> sampledImages,
                                        List<Texture> storageImages)
    {
        if (element.Type
            is ResourceType.ConstantBuffer
            or ResourceType.StructuredBuffer
            or ResourceType.StructuredBufferReadWrite)
        {
            DescriptorBufferInfo* infos = allocator.Alloc<DescriptorBufferInfo>(element.Count);

            for (uint i = 0; i < element.Count; i++)
            {
                Buffer buffer = (Buffer)resources[i];

                infos[i] = new()
                {
                    Buffer = buffer.VK().Buffer,
                    Offset = 0,
                    Range = element.Range is 0 ? Vk.WholeSize : element.Range
                };
            }

            write.PBufferInfo = infos;
        }
        else if (element.Type is ResourceType.Texture or ResourceType.TextureReadWrite)
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
        else if (element.Type is ResourceType.Sampler)
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
