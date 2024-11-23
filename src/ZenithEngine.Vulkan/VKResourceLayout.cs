using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKResourceLayout : ResourceLayout
{
    public VkDescriptorSetLayout DescriptorSetLayout;

    public VKResourceLayout(GraphicsContext context,
                            ref readonly ResourceLayoutDesc desc) : base(context, in desc)
    {
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSetLayout, DescriptorSetLayout.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyDescriptorSetLayout(Context.Device, DescriptorSetLayout, null);
    }
}
