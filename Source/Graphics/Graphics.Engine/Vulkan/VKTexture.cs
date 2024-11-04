using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKTexture : Texture
{
    public VKTexture(Context context,
                     ref readonly TextureDescription description) : base(context, in description)
    {
        bool isCube = description.Type == TextureType.TextureCube;

        ImageCreateInfo createInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = Formats.GetImageType(description.Type),
            Format = Formats.GetPixelFormat(description.Format,
                                            description.Usage.HasFlag(TextureUsage.DepthStencil)),
            Extent = new Extent3D
            {
                Width = description.Width,
                Height = description.Height,
                Depth = description.Depth
            },
            MipLevels = description.MipLevels,
            ArrayLayers = isCube ? 6u : 1u,
            Samples = Formats.GetSampleCountFlags(description.SampleCount),
            Tiling = ImageTiling.Optimal,
            Usage = Formats.GetImageUsageFlags(description.Usage),
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

        Layouts = new ImageLayout[description.Depth * description.MipLevels * createInfo.ArrayLayers];
        Array.Fill(Layouts, ImageLayout.Preinitialized);

        Image = image;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkImage Image { get; }

    public VKDeviceMemory DeviceMemory { get; }

    public ImageLayout[] Layouts { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Image, Image.Handle, name);
        Context.SetDebugName(ObjectType.DeviceMemory, DeviceMemory.DeviceMemory.Handle, $"{name} Memory");
    }

    protected override void Destroy()
    {
        DeviceMemory.Dispose();

        Context.Vk.DestroyImage(Context.Device, Image, null);
    }
}
