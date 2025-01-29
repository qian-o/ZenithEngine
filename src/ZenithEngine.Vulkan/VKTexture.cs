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
            createInfo.QueueFamilyIndexCount = (uint)Context.QueueFamilyIndices!.Length;
            createInfo.PQueueFamilyIndices = Allocator.Alloc(Context.QueueFamilyIndices);
        }

        Context.Vk.CreateImage(Context.Device, &createInfo, null, out Image).ThrowIfError();

        ImageMemoryRequirementsInfo2 requirementsInfo2 = new()
        {
            SType = StructureType.ImageMemoryRequirementsInfo2,
            Image = Image
        };

        MemoryRequirements2 requirements2 = new()
        {
            SType = StructureType.MemoryRequirements2
        };

        requirements2.AddNext(out MemoryDedicatedRequirements dedicatedRequirements);

        Context.Vk.GetImageMemoryRequirements2(Context.Device, &requirementsInfo2, &requirements2);

        DeviceMemory = new(Context,
                           false,
                           requirements2.MemoryRequirements,
                           dedicatedRequirements.PrefersDedicatedAllocation || dedicatedRequirements.RequiresDedicatedAllocation,
                           Image,
                           null);

        Context.Vk.BindImageMemory(Context.Device,
                                   Image,
                                   DeviceMemory.DeviceMemory,
                                   0).ThrowIfError();

        ImageView = CreateImageView(0,
                                    desc.MipLevels,
                                    CubeMapFace.PositiveX,
                                    VKHelpers.GetArrayLayers(desc));

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];
        Array.Fill(imageLayouts, ImageLayout.Undefined);

        Allocator.Release();
    }

    public VKTexture(GraphicsContext context,
                     ref readonly TextureDesc desc,
                     VkImage image) : base(context, in desc)
    {
        Image = image;

        ImageView = CreateImageView(0,
                                    desc.MipLevels,
                                    CubeMapFace.PositiveX,
                                    VKHelpers.GetArrayLayers(desc));

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];
        Array.Fill(imageLayouts, ImageLayout.Undefined);
    }

    public ImageLayout this[uint mipLevel, CubeMapFace face]
    {
        get
        {
            return imageLayouts[VKHelpers.GetArrayLayerIndex(Desc, mipLevel, face)];
        }
        private set
        {
            imageLayouts[VKHelpers.GetArrayLayerIndex(Desc, mipLevel, face)] = value;
        }
    }

    public VKDeviceMemory? DeviceMemory { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public VkImageView CreateImageView(uint baseMipLevel,
                                       uint mipLevels,
                                       CubeMapFace baseFace,
                                       uint faceCount)
    {
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = Image,
            ViewType = VKFormats.GetImageViewType(Desc.Type),
            Format = VKFormats.GetPixelFormat(Desc.Format),
            SubresourceRange = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(Desc.Usage),
                BaseMipLevel = baseMipLevel,
                LevelCount = mipLevels,
                BaseArrayLayer = (uint)baseFace,
                LayerCount = faceCount
            }
        };

        VkImageView imageView;
        Context.Vk.CreateImageView(Context.Device, &createInfo, null, &imageView).ThrowIfError();

        return imageView;
    }

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

        for (uint i = 0; i < mipLevels; i++)
        {
            uint mipLevel = baseMipLevel + i;

            for (uint j = 0; j < faceCount; j++)
            {
                uint face = (uint)baseFace + j;

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
        Context.Vk.DestroyImageView(Context.Device, ImageView, null);

        if (DeviceMemory is not null)
        {
            DeviceMemory.Dispose();

            Context.Vk.DestroyImage(Context.Device, Image, null);
        }
    }
}
