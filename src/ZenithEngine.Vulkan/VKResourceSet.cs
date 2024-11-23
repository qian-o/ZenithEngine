using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal class VKResourceSet : ResourceSet
{
    public VkDescriptorSet DescriptorSet;

    public VKResourceSet(GraphicsContext context,
                         ref readonly ResourceSetDesc desc) : base(context, in desc)
    {
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSet, DescriptorSet.Handle, name);
    }

    protected override void Destroy()
    {
    }
}
