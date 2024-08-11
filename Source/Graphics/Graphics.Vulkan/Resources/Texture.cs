using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Texture : DeviceResource, IBindableResource
{
    private readonly VkImage _image;
    private readonly TextureType _type;
    private readonly PixelFormat _format;
    private readonly Format _vkFormat;
    private readonly TextureSampleCount _sampleCount;
    private readonly SampleCountFlags _vkSampleCount;
    private readonly TextureUsage _usage;
    private readonly uint _width;
    private readonly uint _height;
    private readonly uint _depth;
    private readonly uint _mipLevels;
    private readonly uint _arrayLayers;
    private readonly ImageLayout[] _imageLayouts;
    private readonly DeviceMemory? _deviceMemory;
    private readonly bool _isSwapchainImage;

    internal Texture(GraphicsDevice graphicsDevice, ref readonly TextureDescription description) : base(graphicsDevice)
    {
        bool isCube = description.Usage.HasFlag(TextureUsage.Cubemap);
        uint arrayLayers = (isCube ? 6u : 1u) * description.Depth;
        uint subresourceCount = arrayLayers * description.MipLevels;

        ImageCreateInfo createInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = Formats.GetImageType(description.Type),
            Extent = new Extent3D
            {
                Width = description.Width,
                Height = description.Height,
                Depth = description.Depth
            },
            MipLevels = description.MipLevels,
            ArrayLayers = arrayLayers,
            InitialLayout = ImageLayout.Preinitialized,
            Usage = Formats.GetImageUsageFlags(description.Usage),
            Tiling = ImageTiling.Optimal,
            Format = Formats.GetPixelFormat(description.Format, description.Usage.HasFlag(TextureUsage.DepthStencil)),
            Flags = ImageCreateFlags.CreateMutableFormatBit,
            Samples = Formats.GetSampleCount(description.SampleCount)
        };

        if (isCube)
        {
            createInfo.Flags |= ImageCreateFlags.CreateCubeCompatibleBit;
        }

        VkImage image;
        Vk.CreateImage(Device, &createInfo, null, &image).ThrowCode();

        MemoryRequirements memoryRequirements;
        Vk.GetImageMemoryRequirements(Device, image, &memoryRequirements);

        bool isStaging = description.Usage.HasFlag(TextureUsage.Staging);

        DeviceMemory deviceMemory = new(graphicsDevice,
                                        in memoryRequirements,
                                        isStaging ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit);

        Vk.BindImageMemory(Device, image, deviceMemory.Handle, 0).ThrowCode();

        ImageLayout[] imageLayouts = new ImageLayout[subresourceCount];
        Array.Fill(imageLayouts, ImageLayout.Preinitialized);

        _image = image;
        _type = description.Type;
        _format = description.Format;
        _vkFormat = createInfo.Format;
        _sampleCount = description.SampleCount;
        _vkSampleCount = createInfo.Samples;
        _usage = description.Usage;
        _width = description.Width;
        _height = description.Height;
        _depth = description.Depth;
        _mipLevels = description.MipLevels;
        _arrayLayers = createInfo.ArrayLayers;
        _imageLayouts = imageLayouts;
        _deviceMemory = deviceMemory;
        _isSwapchainImage = false;
    }

    internal Texture(GraphicsDevice graphicsDevice, VkImage image, Format format, uint width, uint height) : base(graphicsDevice)
    {
        _image = image;
        _type = TextureType.Texture2D;
        _format = Formats.GetPixelFormat(format);
        _vkFormat = format;
        _sampleCount = TextureSampleCount.Count1;
        _vkSampleCount = SampleCountFlags.Count1Bit;
        _usage = TextureUsage.RenderTarget;
        _width = width;
        _height = height;
        _depth = 1;
        _mipLevels = 1;
        _arrayLayers = 1;
        _imageLayouts = [ImageLayout.Undefined];
        _deviceMemory = null;
        _isSwapchainImage = true;
    }

    internal VkImage Handle => _image;

    internal TextureType Type => _type;

    internal PixelFormat Format => _format;

    internal Format VkFormat => _vkFormat;

    internal TextureSampleCount SampleCount => _sampleCount;

    internal SampleCountFlags VkSampleCount => _vkSampleCount;

    internal TextureUsage Usage => _usage;

    public uint Width => _width;

    public uint Height => _height;

    public uint Depth => _depth;

    public uint MipLevels => _mipLevels;

    public uint ArrayLayers => _arrayLayers;

    internal void TransitionLayout(CommandBuffer commandBuffer, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ImageLayout newLayout)
    {
        for (uint level = baseMipLevel; level < baseMipLevel + levelCount; level++)
        {
            for (uint layer = baseArrayLayer; layer < baseArrayLayer + layerCount; layer++)
            {
                uint index = (layer * _mipLevels) + level;

                ImageLayout oldLayout = _imageLayouts[index];

                if (oldLayout != newLayout)
                {
                    ImageMemoryBarrier barrier = new()
                    {
                        SType = StructureType.ImageMemoryBarrier,
                        Image = _image,
                        SubresourceRange = new ImageSubresourceRange
                        {
                            BaseMipLevel = level,
                            LevelCount = 1,
                            BaseArrayLayer = layer,
                            LayerCount = 1
                        },
                        OldLayout = oldLayout,
                        NewLayout = newLayout,
                    };

                    if (newLayout == ImageLayout.DepthStencilAttachmentOptimal)
                    {
                        barrier.SubresourceRange.AspectMask = ImageAspectFlags.DepthBit;

                        if (HasStencilComponent(_format))
                        {
                            barrier.SubresourceRange.AspectMask |= ImageAspectFlags.StencilBit;
                        }
                    }
                    else
                    {
                        barrier.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
                    }

                    PipelineStageFlags srcStageFlags;
                    PipelineStageFlags dstStageFlags;

                    // Transition layouts.
                    {
                        if (oldLayout == ImageLayout.Undefined)
                        {
                            barrier.SrcAccessMask = AccessFlags.None;
                            srcStageFlags = PipelineStageFlags.TopOfPipeBit;
                        }
                        else if (oldLayout == ImageLayout.Preinitialized)
                        {
                            barrier.SrcAccessMask = AccessFlags.HostWriteBit;
                            srcStageFlags = PipelineStageFlags.HostBit;
                        }
                        else if (oldLayout == ImageLayout.TransferSrcOptimal)
                        {
                            barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                            srcStageFlags = PipelineStageFlags.TransferBit;
                        }
                        else if (oldLayout == ImageLayout.TransferDstOptimal)
                        {
                            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                            srcStageFlags = PipelineStageFlags.TransferBit;
                        }
                        else if (oldLayout == ImageLayout.ShaderReadOnlyOptimal)
                        {
                            barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
                            srcStageFlags = PipelineStageFlags.FragmentShaderBit;
                        }
                        else if (oldLayout == ImageLayout.ColorAttachmentOptimal)
                        {
                            barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                            srcStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                        }
                        else if (oldLayout == ImageLayout.DepthStencilAttachmentOptimal)
                        {
                            barrier.SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit;
                            srcStageFlags = PipelineStageFlags.EarlyFragmentTestsBit;
                        }
                        else if (oldLayout == ImageLayout.PresentSrcKhr)
                        {
                            barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                            srcStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported layout transition.");
                        }

                        if (newLayout == ImageLayout.Undefined)
                        {
                            barrier.DstAccessMask = AccessFlags.None;
                            dstStageFlags = PipelineStageFlags.TopOfPipeBit;
                        }
                        else if (newLayout == ImageLayout.Preinitialized)
                        {
                            barrier.DstAccessMask = AccessFlags.HostWriteBit;
                            dstStageFlags = PipelineStageFlags.HostBit;
                        }
                        else if (newLayout == ImageLayout.TransferSrcOptimal)
                        {
                            barrier.DstAccessMask = AccessFlags.TransferReadBit;
                            dstStageFlags = PipelineStageFlags.TransferBit;
                        }
                        else if (newLayout == ImageLayout.TransferDstOptimal)
                        {
                            barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                            dstStageFlags = PipelineStageFlags.TransferBit;
                        }
                        else if (newLayout == ImageLayout.ShaderReadOnlyOptimal)
                        {
                            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                            dstStageFlags = PipelineStageFlags.FragmentShaderBit;
                        }
                        else if (newLayout == ImageLayout.ColorAttachmentOptimal)
                        {
                            barrier.DstAccessMask = AccessFlags.ColorAttachmentWriteBit;
                            dstStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                        }
                        else if (newLayout == ImageLayout.DepthStencilAttachmentOptimal)
                        {
                            barrier.DstAccessMask = AccessFlags.DepthStencilAttachmentWriteBit;
                            dstStageFlags = PipelineStageFlags.EarlyFragmentTestsBit;
                        }
                        else if (newLayout == ImageLayout.PresentSrcKhr)
                        {
                            barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                            dstStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported layout transition.");
                        }
                    }

                    Vk.CmdPipelineBarrier(commandBuffer,
                                          srcStageFlags,
                                          dstStageFlags,
                                          DependencyFlags.None,
                                          0,
                                          null,
                                          0,
                                          null,
                                          1,
                                          &barrier);

                    _imageLayouts[index] = newLayout;
                }
            }
        }
    }

    internal void TransitionLayout(CommandBuffer commandBuffer, ImageLayout newLayout)
    {
        TransitionLayout(commandBuffer, 0, _mipLevels, 0, _arrayLayers, newLayout);
    }

    internal void TransitionToBestLayout(CommandBuffer commandBuffer)
    {
        ImageLayout newLayout = ImageLayout.General;

        if (Usage.HasFlag(TextureUsage.Sampled))
        {
            newLayout = ImageLayout.ShaderReadOnlyOptimal;
        }
        else if (Usage.HasFlag(TextureUsage.RenderTarget))
        {
            newLayout = ImageLayout.ColorAttachmentOptimal;
        }
        else if (Usage.HasFlag(TextureUsage.DepthStencil))
        {
            newLayout = ImageLayout.DepthStencilAttachmentOptimal;
        }

        TransitionLayout(commandBuffer, newLayout);
    }

    protected override void Destroy()
    {
        if (!_isSwapchainImage)
        {
            Vk.DestroyImage(Device, _image, null);

            _deviceMemory!.Dispose();
        }
    }

    private static bool HasStencilComponent(PixelFormat format)
    {
        return format == PixelFormat.D24UNormS8UInt || format == PixelFormat.D32FloatS8UInt;
    }
}
