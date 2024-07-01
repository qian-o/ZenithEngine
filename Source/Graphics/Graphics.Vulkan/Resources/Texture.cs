using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public class Texture : DeviceResource
{
    internal Texture(GraphicsDevice graphicsDevice, in TextureDescription description) : base(graphicsDevice)
    {
        ImageCreateInfo imageCreateInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = Formats.GetImageType(description.Type),
            Extent = new Extent3D
            {
                Width = description.Width,
                Height = description.Height,
                Depth = description.Depth
            },
            InitialLayout = ImageLayout.Preinitialized,
            Usage = Formats.GetImageUsageFlags(description.Usage),
            Tiling = description.Usage.HasFlag(TextureUsage.Staging) ? ImageTiling.Linear : ImageTiling.Optimal,
            Format = Formats.GetPixelFormat(description.Format, description.Usage.HasFlag(TextureUsage.DepthStencil)),
            Flags = ImageCreateFlags.CreateMutableFormatBit,
            Samples = Formats.GetSampleCount(description.SampleCount)
        };

        if (description.Usage.HasFlag(TextureUsage.Cubemap))
        {
            imageCreateInfo.Flags |= ImageCreateFlags.CreateCubeCompatibleBit;
        }
    }

    protected override void Destroy()
    {
    }
}
