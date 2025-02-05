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
        bool isCube = desc.Type is TextureType.TextureCube or TextureType.TextureCubeArray;

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
            Flags = isCube ? ImageCreateFlags.CreateCubeCompatibleBit : ImageCreateFlags.None
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

        ImageView = CreateImageView();

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];
        Array.Fill(imageLayouts, ImageLayout.Undefined);

        Allocator.Release();
    }

    public VKTexture(GraphicsContext context,
                     ref readonly TextureDesc desc,
                     VkImage image) : base(context, in desc)
    {
        Image = image;

        ImageView = CreateImageView();

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];
        Array.Fill(imageLayouts, ImageLayout.Undefined);
    }

    public ImageLayout this[uint mipLevel, uint arrayLayer, CubeMapFace face]
    {
        get
        {
            return imageLayouts[VKHelpers.GetArrayLayerIndex(Desc, mipLevel, arrayLayer, face)];
        }
        private set
        {
            imageLayouts[VKHelpers.GetArrayLayerIndex(Desc, mipLevel, arrayLayer, face)] = value;
        }
    }

    public VKDeviceMemory? DeviceMemory { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void TransitionLayout(VkCommandBuffer commandBuffer,
                                 uint baseMipLevel,
                                 uint mipLevels,
                                 uint baseArrayLayer,
                                 uint arrayLayers,
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

            for (uint j = 0; j < arrayLayers; j++)
            {
                uint arrayLayer = baseArrayLayer + j;

                for (uint k = 0; k < faceCount; k++)
                {
                    uint face = (uint)baseFace + k;

                    ImageLayout oldLayout = this[mipLevel, arrayLayer, (CubeMapFace)face];

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

                    this[mipLevel, arrayLayer, (CubeMapFace)face] = newLayout;
                }
            }
        }
    }

    public void TransitionLayout(VkCommandBuffer commandBuffer, ImageLayout newLayout)
    {
        TransitionLayout(commandBuffer,
                         0,
                         Desc.MipLevels,
                         0,
                         Desc.ArrayLayers,
                         CubeMapFace.PositiveX,
                         VKHelpers.GetInitialLayers(Desc.Type),
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

    private VkImageView CreateImageView()
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
                BaseMipLevel = 0,
                LevelCount = Desc.MipLevels,
                BaseArrayLayer = 0,
                LayerCount = VKHelpers.GetArrayLayers(Desc)
            }
        };

        VkImageView imageView;
        Context.Vk.CreateImageView(Context.Device, &createInfo, null, &imageView).ThrowIfError();

        return imageView;
    }
}
