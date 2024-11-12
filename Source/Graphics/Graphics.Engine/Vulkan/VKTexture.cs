using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

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

    public void TransitionImageLayout(VkCommandBuffer commandBuffer,
                                      ImageLayout newLayout,
                                      uint baseMipLevel,
                                      uint levelCount,
                                      CubeMapFace baseFace,
                                      uint faceCount)
    {
        bool isCube = Desc.Type == TextureType.TextureCube;

        uint index = isCube ? ((uint)baseFace * Desc.MipLevels) + baseMipLevel : baseMipLevel;

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = Layouts[index],
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

        Layouts[index] = newLayout;
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
