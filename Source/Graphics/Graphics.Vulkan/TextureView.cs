using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class TextureView : DeviceResource, IBindableResource
{
    private readonly VkImageView _imageView;

    internal TextureView(GraphicsDevice graphicsDevice, ref readonly TextureViewDescription description) : base(graphicsDevice)
    {
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = description.Target.Handle,
            Format = Formats.GetPixelFormat(description.Target.Format, description.Target.Usage.HasFlag(TextureUsage.DepthStencil))
        };

        ImageAspectFlags aspectFlags = description.Target.Usage.HasFlag(TextureUsage.DepthStencil) ? ImageAspectFlags.DepthBit : ImageAspectFlags.ColorBit;

        createInfo.SubresourceRange = new ImageSubresourceRange
        {
            AspectMask = aspectFlags,
            BaseMipLevel = description.BaseMipLevel,
            LevelCount = description.MipLevels,
            BaseArrayLayer = description.BaseArrayLayer,
            LayerCount = description.ArrayLayers
        };

        if (description.Target.Usage.HasFlag(TextureUsage.Cubemap))
        {
            createInfo.ViewType = ImageViewType.TypeCube;
        }
        else
        {
            switch (description.Target.Type)
            {
                case TextureType.Texture1D:
                    createInfo.ViewType = ImageViewType.Type1D;
                    break;
                case TextureType.Texture2D:
                    createInfo.ViewType = ImageViewType.Type2D;
                    break;
                case TextureType.Texture3D:
                    createInfo.ViewType = ImageViewType.Type3D;
                    break;
            }
        }

        VkImageView imageView;
        Vk.CreateImageView(Device, &createInfo, null, &imageView).ThrowCode();

        _imageView = imageView;
    }

    internal VkImageView Handle => _imageView;

    protected override void Destroy()
    {
        Vk.DestroyImageView(Device, _imageView, null);
    }
}
