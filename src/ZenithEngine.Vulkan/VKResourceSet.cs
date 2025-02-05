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
        Token = Context.DescriptorSetAllocator!.Alloc(desc.Layout.VK());

        uint resourceOffset = 0;
        List<VKTexture> srvTextures = [];
        List<VKTexture> uavTextures = [];

        ResourceLayoutDesc layoutDesc = desc.Layout.Desc;

        WriteDescriptorSet[] writes = new WriteDescriptorSet[layoutDesc.Elements.Length];

        for (int i = 0; i < layoutDesc.Elements.Length; i++)
        {
            LayoutElementDesc element = layoutDesc.Elements[i];
            GraphicsResource[] resources = desc.Resources[(int)resourceOffset..(int)(resourceOffset + element.Count)];

            WriteDescriptorSet write = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = Token.Set,
                DstBinding = VKHelpers.GetBinding(element.Type, element.Slot),
                DescriptorCount = element.Count,
                DescriptorType = VKFormats.GetDescriptorType(element.Type, element.AllowDynamicOffset)
            };

            if (element.Type
                is ResourceType.ConstantBuffer
                or ResourceType.StructuredBuffer
                or ResourceType.StructuredBufferReadWrite)
            {
                DescriptorBufferInfo* infos = Allocator.Alloc<DescriptorBufferInfo>(element.Count);

                for (uint j = 0; j < element.Count; j++)
                {
                    VKBuffer buffer = (VKBuffer)resources[j];

                    infos[j] = new()
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
                DescriptorImageInfo* infos = Allocator.Alloc<DescriptorImageInfo>(element.Count);

                bool isSrv = element.Type is ResourceType.Texture;
                ImageLayout imageLayout = isSrv ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General;

                for (uint j = 0; j < element.Count; j++)
                {
                    VKTexture texture = (VKTexture)resources[j];

                    infos[j] = new()
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
                DescriptorImageInfo* infos = Allocator.Alloc<DescriptorImageInfo>(element.Count);

                for (uint j = 0; j < element.Count; j++)
                {
                    VKSampler sampler = (VKSampler)resources[j];

                    infos[j] = new()
                    {
                        Sampler = sampler.Sampler
                    };
                }

                write.PImageInfo = infos;
            }
            else if (element.Type is ResourceType.AccelerationStructure)
            {
                WriteDescriptorSetAccelerationStructureKHR descriptorSetAccelerationStructure = new()
                {
                    SType = StructureType.WriteDescriptorSetAccelerationStructureKhr,
                    AccelerationStructureCount = element.Count
                };

                AccelerationStructureKHR* accelerationStructures = Allocator.Alloc<AccelerationStructureKHR>(element.Count);

                for (uint j = 0; j < element.Count; j++)
                {
                    VKTopLevelAS topLevelAS = (VKTopLevelAS)resources[j];

                    accelerationStructures[j] = topLevelAS.AccelerationStructure;
                }

                descriptorSetAccelerationStructure.PAccelerationStructures = accelerationStructures;

                write.PNext = Allocator.Alloc([descriptorSetAccelerationStructure]);
            }
            else
            {
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(element.Type));
            }

            writes[i] = write;

            resourceOffset += element.Count;
        }

        Context.Vk.UpdateDescriptorSets(Context.Device,
                                        (uint)writes.Length,
                                        writes,
                                        0,
                                        (CopyDescriptorSet*)null);

        DynamicConstantBufferCount = layoutDesc.DynamicConstantBufferCount;
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
}
