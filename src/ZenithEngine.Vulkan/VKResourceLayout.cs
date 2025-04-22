using Silk.NET.Vulkan;
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
                Binding = VKHelpers.GetBinding(element.Type, element.Slot),
                DescriptorType = VKFormats.GetDescriptorType(element.Type, element.AllowDynamicOffset),
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

        Counts = new(uniformBufferCount,
                     storageBufferCount,
                     sampledImageCount,
                     storageImageCount,
                     samplerCount,
                     accelerationStructureCount);

        Allocator.Release();
    }

    public VKResourceCounts Counts { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void SetName(string name)
    {
        DebugUtilsObjectNameInfoEXT nameInfo = new()
        {
            SType = StructureType.DebugUtilsObjectNameInfoExt,
            ObjectType = ObjectType.DescriptorSetLayout,
            ObjectHandle = DescriptorSetLayout.Handle,
            PObjectName = Allocator.AllocUTF8(name)
        };

        Context.ExtDebugUtils!.SetDebugUtilsObjectName(Context.Device, &nameInfo).ThrowIfError();
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyDescriptorSetLayout(Context.Device, DescriptorSetLayout, null);
    }
}
