using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKTexture : Texture
{
    public VkImage Image;
    public VkImageView ImageView;

    private readonly ImageLayout[] imageLayouts;

    public VKTexture(GraphicsContext context,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
        // Image
        {
            ImageCreateInfo createInfo = new()
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = VKFormats.GetImageType(desc.Type),
                Format = VKFormats.GetPixelFormat(desc.Format),
                Extent = new()
                {
                    Width = desc.Width,
                    Height = desc.Height,
                    Depth = desc.Depth
                },
                MipLevels = desc.MipLevels,
                ArrayLayers = VKHelpers.GetArrayLayers(desc),
                Samples = VKFormats.GetSampleCountFlags(desc.SampleCount),
                Tiling = ImageTiling.Optimal,
                Usage = VKFormats.GetImageUsageFlags(desc.Usage),
                SharingMode = Context.SharingEnabled ? SharingMode.Concurrent : SharingMode.Exclusive,
                InitialLayout = ImageLayout.Preinitialized,
                Flags = desc.Type is TextureType.TextureCube ? ImageCreateFlags.CreateCubeCompatibleBit : ImageCreateFlags.None
            };

            if (Context.SharingEnabled)
            {
                createInfo.QueueFamilyIndexCount = 2;
                createInfo.PQueueFamilyIndices = Allocator.Alloc([Context.DirectQueueFamilyIndex, Context.CopyQueueFamilyIndex]);
            }

            Context.Vk.CreateImage(Context.Device, &createInfo, null, out Image).ThrowIfError();

            MemoryRequirements requirements;
            Context.Vk.GetImageMemoryRequirements(Context.Device, Image, &requirements);

            DeviceMemory = new(Context, requirements, false);

            Context.Vk.BindImageMemory(Context.Device,
                                       Image,
                                       DeviceMemory.DeviceMemory,
                                       0).ThrowIfError();
        }

        // Image View
        {
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = Image,
                ViewType = VKFormats.GetImageViewType(desc.Type),
                Format = VKFormats.GetPixelFormat(desc.Format),
                SubresourceRange = new()
                {
                    AspectMask = VKFormats.GetImageAspectFlags(desc.Usage),
                    BaseMipLevel = 0,
                    LevelCount = desc.MipLevels,
                    BaseArrayLayer = 0,
                    LayerCount = VKHelpers.GetArrayLayers(desc)
                }
            };

            Context.Vk.CreateImageView(Context.Device, &createInfo, null, out ImageView).ThrowIfError();
        }

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];

        Allocator.Release();
    }

    public VKTexture(GraphicsContext context,
                     ref readonly TextureDesc desc,
                     VkImage image) : base(context, in desc)
    {
        Image = image;

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];
    }

    public VKDeviceMemory? DeviceMemory { get; }

    public ImageLayout this[uint mipLevel, CubeMapFace face]
    {
        get
        {
            return imageLayouts[(mipLevel * VKHelpers.GetArrayLayers(Desc)) + (uint)face];
        }
        set
        {
            imageLayouts[(mipLevel * VKHelpers.GetArrayLayers(Desc)) + (uint)face] = value;
        }
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void TransitionLayout(VkCommandBuffer commandBuffer,
                                 uint baseMipLevel,
                                 uint mipLevels,
                                 CubeMapFace baseFace,
                                 uint faceCount,
                                 ImageLayout newLayout)
    {
        if (newLayout is ImageLayout.Undefined or ImageLayout.Preinitialized)
        {
            return;
        }

        for (int i = 0; i < mipLevels; i++)
        {
            uint mipLevel = baseMipLevel + (uint)i;

            for (int j = 0; j < faceCount; j++)
            {
                uint face = (uint)baseFace + (uint)j;

                ImageLayout oldLayout = this[mipLevel, (CubeMapFace)face];

                if (oldLayout == newLayout)
                {
                    continue;
                }

                ImageMemoryBarrier barrier = new()
                {
                    SType = StructureType.ImageMemoryBarrier,
                    OldLayout = oldLayout,
                    NewLayout = newLayout,
                    Image = Image,
                    SubresourceRange = new()
                    {
                        AspectMask = VKFormats.GetImageAspectFlags(Desc.Usage),
                        BaseMipLevel = mipLevel,
                        LevelCount = 1,
                        BaseArrayLayer = face,
                        LayerCount = 1
                    }
                };

                VKHelpers.MatchImageLayout(ref barrier, out PipelineStageFlags src, out PipelineStageFlags dst);

                Context.Vk.CmdPipelineBarrier(commandBuffer,
                                              src,
                                              dst,
                                              0,
                                              null,
                                              0,
                                              null,
                                              1,
                                              &barrier);

                this[mipLevel, (CubeMapFace)face] = newLayout;
            }
        }
    }

    public void TransitionLayout(VkCommandBuffer commandBuffer, ImageLayout newLayout)
    {
        TransitionLayout(commandBuffer,
                         0,
                         Desc.MipLevels,
                         CubeMapFace.PositiveX,
                         VKHelpers.GetArrayLayers(Desc),
                         newLayout);
    }

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.Image, Image.Handle, name);
        Context.SetDebugName(ObjectType.ImageView, ImageView.Handle, name);

        if (DeviceMemory is not null)
        {
            DeviceMemory.Name = $"{name} DeviceMemory";
        }
    }

    protected override void Destroy()
    {
        DeviceMemory?.Dispose();

        Context.Vk.DestroyImageView(Context.Device, ImageView, null);
        Context.Vk.DestroyImage(Context.Device, Image, null);
    }
}
