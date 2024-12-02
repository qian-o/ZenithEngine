﻿using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKResourceLayout : ResourceLayout
{
    public VkDescriptorSetLayout DescriptorSetLayout;

    public VKResourceLayout(GraphicsContext context,
                            ref readonly ResourceLayoutDesc desc) : base(context, in desc)
    {
        DescriptorSetLayoutBinding* bindings = Allocator.Alloc<DescriptorSetLayoutBinding>((uint)desc.Elements.Length);

        uint uniformBufferCount = 0;
        uint storageBufferCount = 0;
        uint sampledImageCount = 0;
        uint storageImageCount = 0;
        uint samplerCount = 0;
        uint accelerationStructureCount = 0;

        for (int i = 0; i < desc.Elements.Length; i++)
        {
            LayoutElementDesc element = desc.Elements[i];

            bindings[i] = new()
            {
                Binding = Utils.GetBinding(element.Type, element.Slot),
                DescriptorType = VKFormats.GetDescriptorType(element.Type, element.Options),
                DescriptorCount = element.Count,
                StageFlags = VKFormats.GetShaderStageFlags(element.Stages)
            };

            switch (element.Type)
            {
                case ResourceType.ConstantBuffer:
                    uniformBufferCount += element.Count;
                    break;
                case ResourceType.StructuredBuffer:
                case ResourceType.StructuredBufferReadWrite:
                    storageBufferCount += element.Count;
                    break;
                case ResourceType.Texture:
                    sampledImageCount += element.Count;
                    break;
                case ResourceType.TextureReadWrite:
                    storageImageCount += element.Count;
                    break;
                case ResourceType.Sampler:
                    samplerCount += element.Count;
                    break;
                case ResourceType.AccelerationStructure:
                    accelerationStructureCount += element.Count;
                    break;
            }
        }

        DescriptorSetLayoutCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = (uint)desc.Elements.Length,
            PBindings = bindings
        };

        Context.Vk.CreateDescriptorSetLayout(Context.Device,
                                             &createInfo,
                                             null,
                                             out DescriptorSetLayout).ThrowIfError();

        Allocator.Free(bindings);

        Counts = new(uniformBufferCount,
                     storageBufferCount,
                     sampledImageCount,
                     storageImageCount,
                     samplerCount,
                     accelerationStructureCount);
    }

    public VKResourceCounts Counts { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSetLayout, DescriptorSetLayout.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyDescriptorSetLayout(Context.Device, DescriptorSetLayout, null);
    }
}
