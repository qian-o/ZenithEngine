using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class TextureView : DeviceResource
{
    private readonly VkImageView _imageView;

    public TextureView(GraphicsDevice graphicsDevice, in TextureViewDescription description) : base(graphicsDevice)
    {
        ImageViewCreateInfo imageViewCreateInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = description.Target.Handle,
            Format = Formats.GetPixelFormat(description.Target.Format, description.Target.Usage.HasFlag(TextureUsage.DepthStencil))
        };

        ImageAspectFlags aspectFlags = description.Target.Usage.HasFlag(TextureUsage.DepthStencil) ? ImageAspectFlags.DepthBit : ImageAspectFlags.ColorBit;

        imageViewCreateInfo.SubresourceRange = new ImageSubresourceRange
        {
            AspectMask = aspectFlags,
            BaseMipLevel = description.BaseMipLevel,
            LevelCount = description.MipLevels,
            BaseArrayLayer = description.BaseArrayLayer,
            LayerCount = description.ArrayLayers
        };

        if (description.Target.Usage.HasFlag(TextureUsage.Cubemap))
        {
            imageViewCreateInfo.ViewType = ImageViewType.TypeCube;
        }
        else
        {
            switch (description.Target.Type)
            {
                case TextureType.Texture1D:
                    imageViewCreateInfo.ViewType = ImageViewType.Type1D;
                    break;
                case TextureType.Texture2D:
                    imageViewCreateInfo.ViewType = ImageViewType.Type2D;
                    break;
                case TextureType.Texture3D:
                    imageViewCreateInfo.ViewType = ImageViewType.Type3D;
                    break;
            }
        }

        VkImageView imageView;
        Vk.CreateImageView(Device, &imageViewCreateInfo, null, &imageView);

        _imageView = imageView;
    }

    public VkImageView Handle => _imageView;

    protected override void Destroy()
    {
        Vk.DestroyImageView(Device, _imageView, null);
    }
}
