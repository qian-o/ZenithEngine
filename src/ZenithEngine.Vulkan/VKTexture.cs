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
            Flags = desc.Type == TextureType.TextureCube ? ImageCreateFlags.CreateCubeCompatibleBit : ImageCreateFlags.None
        };

        Context.Vk.CreateImage(Context.Device, &createInfo, null, out Image).ThrowIfError();

        MemoryRequirements requirements;
        Context.Vk.GetImageMemoryRequirements(Context.Device, Image, &requirements);

        DeviceMemory = new(Context, requirements, false);

        Context.Vk.BindImageMemory(Context.Device,
                                   Image,
                                   DeviceMemory.DeviceMemory,
                                   0).ThrowIfError();

        imageLayouts = new ImageLayout[desc.MipLevels * VKHelpers.GetArrayLayers(desc)];
    }

    public new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public VKDeviceMemory DeviceMemory { get; }

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

    protected override void DebugName(string name)
    {
        Context.SetDebugName(ObjectType.Image, Image.Handle, name);

        DeviceMemory.Name = $"{name} DeviceMemory";
    }

    protected override void Destroy()
    {
        DeviceMemory.Dispose();

        Context.Vk.DestroyImage(Context.Device, Image, null);
    }
}
