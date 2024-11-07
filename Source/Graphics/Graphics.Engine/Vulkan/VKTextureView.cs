using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKTextureView : TextureView
{
    public VKTextureView(Context context,
                         ref readonly TextureViewDescription description) : base(context, in description)
    {
        TextureDescription texture = description.Target.Description;

        bool isCube = texture.Type == TextureType.TextureCube;

        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            ViewType = Formats.GetImageViewType(texture.Type),
            Image = description.Target.VK().Image,
            Format = Formats.GetPixelFormat(texture.Format),
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = texture.Usage.HasFlag(TextureUsage.DepthStencil) ? ImageAspectFlags.DepthBit : ImageAspectFlags.ColorBit,
                BaseMipLevel = description.BaseMipLevel,
                LevelCount = description.MipLevels,
                BaseArrayLayer = isCube ? (uint)description.BaseFace : 0u,
                LayerCount = isCube ? description.FaceCount : 1u
            }
        };

        VkImageView imageView;
        Context.Vk.CreateImageView(Context.Device, &createInfo, null, &imageView).ThrowCode();

        ImageView = imageView;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkImageView ImageView { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.ImageView, ImageView.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyImageView(Context.Device, ImageView, null);
    }
}
