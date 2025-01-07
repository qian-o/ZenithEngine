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

        Token = Context.DescriptorSetAllocator!.Alloc(layout.DescriptorSetLayout, layout.Counts);

        WriteDescriptorSet[] writes = new WriteDescriptorSet[layout.Desc.Elements.Length];

        uint resourceOffset = 0;
        List<VKTexture> srvTextures = [];
        List<VKTexture> uavTextures = [];

        for (uint i = 0; i < writes.Length; i++)
        {
            LayoutElementDesc element = layout.Desc.Elements[i];

            writes[i] = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = Token.Set,
                DstBinding = VKHelpers.GetBinding(element.Type, element.Slot),
                DescriptorCount = element.Count,
                DescriptorType = VKFormats.GetDescriptorType(element.Type, element.AllowDynamicOffset)
            };

            FillDescriptors(Allocator,
                            element,
                            desc.Resources[(int)resourceOffset..(int)(resourceOffset + element.Count)],
                            ref writes[i],
                            srvTextures,
                            uavTextures);

            resourceOffset += element.Count;
        }

        Context.Vk.UpdateDescriptorSets(Context.Device,
                                        (uint)writes.Length,
                                        writes,
                                        0,
                                        (CopyDescriptorSet*)null);

        DynamicConstantBufferCount = layout.Desc.DynamicConstantBufferCount;
        SrvTextures = [.. srvTextures];
        UavTextures = [.. uavTextures];

        Allocator.Release();
    }

    public uint DynamicConstantBufferCount { get; }

    public VKTexture[] SrvTextures { get; }

    public VKTexture[] UavTextures { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSet, Token.Set.Handle, name);
    }

    protected override void Destroy()
    {
        Array.Clear(UavTextures, 0, UavTextures.Length);
        Array.Clear(SrvTextures, 0, SrvTextures.Length);

        Context.DescriptorSetAllocator!.Free(Token);
    }

    private static void FillDescriptors(MemoryAllocator allocator,
                                        LayoutElementDesc element,
                                        GraphicsResource[] resources,
                                        ref WriteDescriptorSet write,
                                        List<VKTexture> srvTextures,
                                        List<VKTexture> uavTextures)
    {
        if (element.Type
            is ResourceType.ConstantBuffer
            or ResourceType.StructuredBuffer
            or ResourceType.StructuredBufferReadWrite)
        {
            DescriptorBufferInfo* infos = allocator.Alloc<DescriptorBufferInfo>(element.Count);

            for (uint i = 0; i < element.Count; i++)
            {
                VKBuffer buffer = (VKBuffer)resources[i];

                infos[i] = new()
                {
                    Buffer = buffer.Buffer,
                    Offset = 0,
                    Range = element.Range is 0 ? Vk.WholeSize : element.Range
                };
            }

            write.PBufferInfo = infos;
        }
        else if (element.Type is ResourceType.Texture or ResourceType.TextureReadWrite)
        {
            DescriptorImageInfo* infos = allocator.Alloc<DescriptorImageInfo>(element.Count);

            bool isSrv = element.Type is ResourceType.Texture;
            ImageLayout imageLayout = isSrv ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General;

            for (uint i = 0; i < element.Count; i++)
            {
                VKTexture texture = (VKTexture)resources[i];

                infos[i] = new()
                {
                    ImageView = texture.ImageView,
                    ImageLayout = imageLayout
                };

                if (isSrv)
                {
                    srvTextures.Add(texture);
                }
                else
                {
                    uavTextures.Add(texture);
                }
            }

            write.PImageInfo = infos;
        }
        else if (element.Type is ResourceType.Sampler)
        {
            DescriptorImageInfo* infos = allocator.Alloc<DescriptorImageInfo>(element.Count);

            for (uint i = 0; i < element.Count; i++)
            {
                VKSampler sampler = (VKSampler)resources[i];

                infos[i] = new()
                {
                    Sampler = sampler.Sampler
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
