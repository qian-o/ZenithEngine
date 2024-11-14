using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;
using System.Runtime.CompilerServices;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKTexture : Texture
{
    public VKTexture(Context context,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
        bool isCube = desc.Type == TextureType.TextureCube;

        ImageCreateInfo createInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = Formats.GetImageType(desc.Type),
            Format = Formats.GetPixelFormat(desc.Format),
            Extent = new Extent3D
            {
                Width = desc.Width,
                Height = desc.Height,
                Depth = desc.Depth
            },
            MipLevels = desc.MipLevels,
            ArrayLayers = isCube ? 6u : 1u,
            Samples = Formats.GetSampleCountFlags(desc.SampleCount),
            Tiling = ImageTiling.Optimal,
            Usage = Formats.GetImageUsageFlags(desc.Usage),
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Preinitialized,
            Flags = isCube ? ImageCreateFlags.CreateCubeCompatibleBit : ImageCreateFlags.None
        };

        VkImage image;
        Context.Vk.CreateImage(Context.Device, &createInfo, null, &image).ThrowCode();

        MemoryRequirements memoryRequirements;
        Context.Vk.GetImageMemoryRequirements(Context.Device, image, &memoryRequirements);

        DeviceMemory = new(Context, false, memoryRequirements);

        Context.Vk.BindImageMemory(Context.Device, image, DeviceMemory.DeviceMemory, 0).ThrowCode();

        Layouts = new ImageLayout[createInfo.ArrayLayers * desc.MipLevels];
        Array.Fill(Layouts, ImageLayout.Preinitialized);

        Image = image;
    }

    public VKTexture(Context context,
                     VkImage image,
                     uint arrayLayers,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
        Image = image;
        Layouts = new ImageLayout[arrayLayers * desc.MipLevels];
        Array.Fill(Layouts, ImageLayout.Undefined);
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkImage Image { get; }

    public VKDeviceMemory? DeviceMemory { get; }

    public ImageLayout[] Layouts { get; }

    public ImageLayout this[uint baseMipLevel, CubeMapFace baseFace]
    {
        get
        {
            bool isCube = Desc.Type == TextureType.TextureCube;
            uint index = isCube ? ((uint)baseFace * Desc.MipLevels) + baseMipLevel : baseMipLevel;

            return Layouts[index];
        }
        set
        {
            bool isCube = Desc.Type == TextureType.TextureCube;
            uint index = isCube ? ((uint)baseFace * Desc.MipLevels) + baseMipLevel : baseMipLevel;

            Layouts[index] = value;
        }
    }

    public void TransitionImageLayout(VkCommandBuffer commandBuffer,
                                      ImageLayout newLayout,
                                      uint baseMipLevel,
                                      uint levelCount,
                                      CubeMapFace baseFace,
                                      uint faceCount)
    {
        bool isCube = Desc.Type == TextureType.TextureCube;

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = this[baseMipLevel, baseFace],
            NewLayout = newLayout,
            Image = Image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = Formats.GetImageAspectFlags(newLayout),
                BaseMipLevel = baseMipLevel,
                LevelCount = levelCount,
                BaseArrayLayer = isCube ? (uint)baseFace : 0,
                LayerCount = isCube ? faceCount : 1
            }
        };

        VKHelpers.GetPipelineStageFlagsForImageLayout(ref barrier,
                                                      out PipelineStageFlags src,
                                                      out PipelineStageFlags dst);

        Context.Vk.CmdPipelineBarrier(commandBuffer,
                                      src,
                                      dst,
                                      DependencyFlags.None,
                                      0,
                                      null,
                                      0,
                                      null,
                                      1,
                                      &barrier);

        this[baseMipLevel, baseFace] = newLayout;
    }

    public void TransitionImageLayout(VkCommandBuffer commandBuffer,
                                      ImageLayout newLayout)
    {
        TransitionImageLayout(commandBuffer, newLayout, 0, Desc.MipLevels, CubeMapFace.PositiveX, 6);
    }

    public void SetData(VkCommandBuffer commandBuffer,
                        nint source,
                        uint sourceSizeInBytes,
                        uint sourceX,
                        uint sourceY,
                        uint sourceZ,
                        uint sourceMipLevel,
                        CubeMapFace sourceBaseFace,
                        uint width,
                        uint height,
                        uint depth)
    {
        Buffer buffer = Context.BufferPool.Buffer(sourceSizeInBytes);

        MappedResource mappedResource = Context.MapMemory(buffer, MapMode.Write);

        Unsafe.CopyBlock((void*)mappedResource.Data,
                         (void*)source,
                         sourceSizeInBytes);

        Context.UnmapMemory(buffer);

        BufferImageCopy bufferImageCopy = new()
        {
            BufferOffset = 0,
            BufferRowLength = 0,
            BufferImageHeight = 0,
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = Formats.GetImageAspectFlags(Desc.Usage),
                MipLevel = sourceMipLevel,
                BaseArrayLayer = (uint)sourceBaseFace,
                LayerCount = 1
            },
            ImageOffset = new Offset3D
            {
                X = (int)sourceX,
                Y = (int)sourceY,
                Z = (int)sourceZ
            },
            ImageExtent = new Extent3D
            {
                Width = width,
                Height = height,
                Depth = depth
            }
        };

        ImageLayout oldLayout = this[sourceMipLevel, sourceBaseFace];

        TransitionImageLayout(commandBuffer,
                              ImageLayout.TransferDstOptimal,
                              sourceMipLevel,
                              1,
                              sourceBaseFace,
                              1);

        Context.Vk.CmdCopyBufferToImage(commandBuffer,
                                        buffer.VK().Buffer,
                                        Image,
                                        ImageLayout.TransferDstOptimal,
                                        1,
                                        &bufferImageCopy);

        TransitionImageLayout(commandBuffer, oldLayout);
    }

    public void CopyTo(VkCommandBuffer commandBuffer,
                       uint sourceX,
                       uint sourceY,
                       uint sourceZ,
                       uint sourceMipLevel,
                       CubeMapFace sourceBaseFace,
                       VKTexture vkDestination,
                       uint destinationX,
                       uint destinationY,
                       uint destinationZ,
                       uint destinationMipLevel,
                       CubeMapFace destinationBaseFace,
                       uint width,
                       uint height,
                       uint depth)
    {
        ImageLayout oldSourceLayout = this[sourceMipLevel, sourceBaseFace];
        ImageLayout oldDestinationLayout = vkDestination[destinationMipLevel, destinationBaseFace];

        TransitionImageLayout(commandBuffer,
                              ImageLayout.TransferSrcOptimal,
                              sourceMipLevel,
                              1,
                              sourceBaseFace,
                              1);

        vkDestination.TransitionImageLayout(commandBuffer,
                                            ImageLayout.TransferDstOptimal,
                                            destinationMipLevel,
                                            1,
                                            destinationBaseFace,
                                            1);

        ImageCopy imageCopy = new()
        {
            SrcSubresource = new ImageSubresourceLayers
            {
                AspectMask = Formats.GetImageAspectFlags(Desc.Usage),
                MipLevel = sourceMipLevel,
                BaseArrayLayer = (uint)sourceBaseFace,
                LayerCount = 1
            },
            SrcOffset = new Offset3D
            {
                X = (int)sourceX,
                Y = (int)sourceY,
                Z = (int)sourceZ
            },
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = Formats.GetImageAspectFlags(vkDestination.Desc.Usage),
                MipLevel = destinationMipLevel,
                BaseArrayLayer = (uint)destinationBaseFace,
                LayerCount = 1
            },
            DstOffset = new Offset3D
            {
                X = (int)destinationX,
                Y = (int)destinationY,
                Z = (int)destinationZ
            },
            Extent = new Extent3D
            {
                Width = width,
                Height = height,
                Depth = depth
            }
        };

        Context.Vk.CmdCopyImage(commandBuffer,
                                Image,
                                ImageLayout.TransferSrcOptimal,
                                vkDestination.Image,
                                ImageLayout.TransferDstOptimal,
                                1,
                                &imageCopy);

        TransitionImageLayout(commandBuffer, oldSourceLayout, sourceMipLevel, 1, sourceBaseFace, 1);

        vkDestination.TransitionImageLayout(commandBuffer,
                                            oldDestinationLayout,
                                            destinationMipLevel,
                                            1,
                                            destinationBaseFace,
                                            1);
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Image, Image.Handle, name);

        if (DeviceMemory != null)
        {
            DeviceMemory.Name = name;
        }
    }

    protected override void Destroy()
    {
        DeviceMemory?.Dispose();

        Context.Vk.DestroyImage(Context.Device, Image, null);
    }
}
