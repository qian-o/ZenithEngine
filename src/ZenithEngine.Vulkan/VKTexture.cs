using Silk.NET.Vulkan;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKTexture : Texture
{
    private readonly ImageLayout[] imageLayouts;

    public VkImage Image;

    public VKTexture(GraphicsContext context,
                     ref readonly TextureDesc desc) : base(context, in desc)
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

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];

        Allocator.Release();
    }

    public VKTexture(GraphicsContext context,
                     ref readonly TextureDesc desc,
                     VkImage image) : base(context, in desc)
    {
        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];

        Image = image;
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

        if (DeviceMemory is not null)
        {
            DeviceMemory.Name = $"{name} DeviceMemory";
        }
    }

    protected override void Destroy()
    {
        DeviceMemory?.Dispose();

        Context.Vk.DestroyImage(Context.Device, Image, null);
    }
}
