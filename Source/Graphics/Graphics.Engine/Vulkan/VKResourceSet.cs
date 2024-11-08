using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKResourceSet : ResourceSet
{
    public VKResourceSet(Context context,
                         ref readonly ResourceSetDesc desc) : base(context, in desc)
    {
        VKResourceLayout layout = desc.Layout.VK();
        VkDescriptorSetLayout vkLayout = layout.DescriptorSetLayout;

        DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[layout.Desc.Elements.Length];

        for (uint i = 0; i < layout.Desc.Elements.Length; i++)
        {
            LayoutElementDesc element = layout.Desc.Elements[i];

            DescriptorPoolSize poolSize = new()
            {
                Type = Formats.GetDescriptorType(element.Type, element.Options),
                DescriptorCount = 1
            };

            poolSizes[i] = poolSize;
        }

        DescriptorPoolCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = 1,
            PoolSizeCount = (uint)poolSizes.Length,
            PPoolSizes = poolSizes.AsPointer()
        };

        VkDescriptorPool descriptorPool;
        Context.Vk.CreateDescriptorPool(Context.Device, &createInfo, null, &descriptorPool).ThrowCode();

        DescriptorSetAllocateInfo allocateInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = descriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &vkLayout
        };

        VkDescriptorSet descriptorSet;
        Context.Vk.AllocateDescriptorSets(Context.Device, &allocateInfo, &descriptorSet).ThrowCode();

        // TODO: Write descriptor set.

        DescriptorPool = descriptorPool;
        DescriptorSet = descriptorSet;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkDescriptorPool DescriptorPool { get; }

    public VkDescriptorSet DescriptorSet { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSet, DescriptorSet.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyDescriptorPool(Context.Device, DescriptorPool, null);
    }
}
