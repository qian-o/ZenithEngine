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
    private readonly DeviceMemory? _deviceMemory;
    private readonly bool _isSwapchainImage;

    private ImageLayout _layout;

    internal Texture(GraphicsDevice graphicsDevice, ref readonly TextureDescription description) : base(graphicsDevice)
    {
        bool isCube = description.Usage.HasFlag(TextureUsage.Cubemap);

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
            ArrayLayers = (isCube ? 6u : 1u) * description.Depth,
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
        _layout = ImageLayout.Preinitialized;
        _mipLevels = description.MipLevels;
        _arrayLayers = createInfo.ArrayLayers;
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
        _mipLevels = 1;
        _arrayLayers = 1;
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

    internal void TransitionLayout(CommandBuffer commandBuffer, ImageLayout newLayout)
    {
        if (_layout == newLayout)
        {
            return;
        }

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            Image = _image,
            SubresourceRange = new ImageSubresourceRange
            {
                BaseMipLevel = 0,
                LevelCount = _mipLevels,
                BaseArrayLayer = 0,
                LayerCount = _arrayLayers
            },
            OldLayout = ImageLayout.Undefined,
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
            if (_layout == ImageLayout.Undefined || _layout == ImageLayout.Preinitialized)
            {
                barrier.SrcAccessMask = AccessFlags.None;
                srcStageFlags = PipelineStageFlags.TopOfPipeBit;
            }
            else if (_layout == ImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else if (_layout == ImageLayout.ColorAttachmentOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                srcStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
            }
            else if (_layout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit;
                srcStageFlags = PipelineStageFlags.EarlyFragmentTestsBit;
            }
            else
            {
                throw new InvalidOperationException("Unsupported layout transition.");
            }

            if (newLayout == ImageLayout.TransferSrcOptimal)
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

        _layout = newLayout;
    }

    internal void TransitionLayout(ImageLayout newLayout)
    {
        CommandBuffer commandBuffer = GraphicsDevice.BeginSingleTimeCommands();

        TransitionLayout(commandBuffer, newLayout);

        GraphicsDevice.EndSingleTimeCommands(commandBuffer);
    }

    internal void TransitionToBestLayout(CommandBuffer? commandBuffer = null)
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

        if (commandBuffer == null)
        {
            TransitionLayout(newLayout);
        }
        else
        {
            TransitionLayout(commandBuffer.Value, newLayout);
        }
    }

    internal void GenerateMipmaps()
    {
        CommandBuffer commandBuffer = GraphicsDevice.BeginSingleTimeCommands();

        for (uint layer = 0; layer < _arrayLayers; layer++)
        {
            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                Image = _image,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseArrayLayer = layer,
                    LayerCount = 1,
                    LevelCount = 1
                }
            };

            int mipWidth = (int)_width;
            int mipHeight = (int)_height;

            for (uint i = 0; i < _mipLevels; i++)
            {
                barrier.SubresourceRange.BaseMipLevel = i;
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                barrier.OldLayout = ImageLayout.TransferDstOptimal;
                barrier.NewLayout = ImageLayout.TransferSrcOptimal;

                Vk.CmdPipelineBarrier(commandBuffer,
                                      PipelineStageFlags.TransferBit,
                                      PipelineStageFlags.TransferBit,
                                      DependencyFlags.None,
                                      0,
                                      null,
                                      0,
                                      null,
                                      1,
                                      &barrier);

                ImageBlit blit = new()
                {
                    SrcOffsets = new ImageBlit.SrcOffsetsBuffer()
                    {
                        Element0 = new Offset3D { X = 0, Y = 0, Z = 0 },
                        Element1 = new Offset3D { X = mipWidth, Y = mipHeight, Z = 1 }
                    },
                    SrcSubresource = new ImageSubresourceLayers
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        MipLevel = i,
                        BaseArrayLayer = layer,
                        LayerCount = 1
                    },
                    DstOffsets = new ImageBlit.DstOffsetsBuffer()
                    {
                        Element0 = new Offset3D { X = 0, Y = 0, Z = 0 },
                        Element1 = new Offset3D { X = Math.Max(1, mipWidth / 2), Y = Math.Max(1, mipHeight / 2), Z = 1 }
                    },
                    DstSubresource = new ImageSubresourceLayers
                    {
                        AspectMask = ImageAspectFlags.ColorBit,
                        MipLevel = i + 1,
                        BaseArrayLayer = layer,
                        LayerCount = 1
                    }
                };

                Vk.CmdBlitImage(commandBuffer,
                                _image,
                                ImageLayout.TransferSrcOptimal,
                                _image,
                                ImageLayout.TransferDstOptimal,
                                1,
                                &blit,
                                Filter.Linear);

                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                barrier.OldLayout = ImageLayout.TransferSrcOptimal;
                barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;

                Vk.CmdPipelineBarrier(commandBuffer,
                                      PipelineStageFlags.TransferBit,
                                      PipelineStageFlags.FragmentShaderBit,
                                      DependencyFlags.None,
                                      0,
                                      null,
                                      0,
                                      null,
                                      1,
                                      &barrier);

                mipWidth = Math.Max(1, mipWidth / 2);
                mipHeight = Math.Max(1, mipHeight / 2);
            }
        }

        GraphicsDevice.EndSingleTimeCommands(commandBuffer);
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
