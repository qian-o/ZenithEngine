using Silk.NET.Vulkan;
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

        for (uint i = 0; i < writes.Length; i++)
        {
            LayoutElementDesc element = layout.Desc.Elements[i];
            GraphicsResource resource = desc.Resources[i];

            WriteDescriptorSet write = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = Token.Set,
                DstBinding = VKHelpers.GetBinding(element),
                DescriptorCount = 1,
                DescriptorType = VKFormats.GetDescriptorType(element.Type, element.Options)
            };

            if (write.DescriptorType
                is DescriptorType.UniformBuffer
                or DescriptorType.UniformBufferDynamic
                or DescriptorType.StorageBuffer
                or DescriptorType.StorageBufferDynamic)
            {
                Buffer buffer = (Buffer)resource;

                DescriptorBufferInfo info = new()
                {
                    Buffer = buffer.VK().Buffer,
                    Offset = 0,
                    Range = element.Size == 0 ? Vk.WholeSize : element.Size
                };

                write.PBufferInfo = &info;

                if (element.Options is ElementOptions.DynamicBinding)
                {
                    dynamicCount++;
                }
            }
            else if (write.DescriptorType is DescriptorType.SampledImage or DescriptorType.StorageImage)
            {
                bool isSampled = element.Type is ResourceType.Texture;

                TextureView textureView = (TextureView)resource;

                DescriptorImageInfo info = new()
                {
                    ImageView = textureView.VK().ImageView,
                    ImageLayout = isSampled ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General
                };

                write.PImageInfo = &info;

                if (isSampled)
                {
                    sampledImages.Add(textureView.Desc.Target);
                }
                else
                {
                    storageImages.Add(textureView.Desc.Target);
                }
            }
            else if (write.DescriptorType is DescriptorType.Sampler)
            {
                Sampler sampler = (Sampler)resource;

                DescriptorImageInfo info = new()
                {
                    Sampler = sampler.VK().Sampler
                };

                write.PImageInfo = &info;
            }
            else
            {
                throw new NotSupportedException("Resource type not supported.");
            }

            writes[i] = write;
        }

        Context.Vk.UpdateDescriptorSets(Context.Device,
                                        (uint)writes.Length,
                                        writes,
                                        0,
                                        (CopyDescriptorSet*)null);

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
}
