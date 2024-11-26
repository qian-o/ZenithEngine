using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKTextureView : TextureView
{
    public VkImageView ImageView;

    public VKTextureView(GraphicsContext context,
                         ref readonly TextureViewDesc desc) : base(context, in desc)
    {
        TextureDesc textureDesc = desc.Target.Desc;

        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = desc.Target.VK().Image,
            ViewType = VKFormats.GetImageViewType(textureDesc.Type),
            Format = VKFormats.GetPixelFormat(desc.Format),
            SubresourceRange = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(textureDesc.Usage),
                BaseMipLevel = desc.BaseMipLevel,
                LevelCount = desc.MipLevels,
                BaseArrayLayer = (uint)desc.BaseFace,
                LayerCount = desc.FaceCount
            }
        };

        Context.Vk.CreateImageView(Context.Device,
                                   &createInfo,
                                   null,
                                   out ImageView).ThrowIfError();
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.ImageView, ImageView.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyImageView(Context.Device, ImageView, null);
    }
}
