using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Texture : DeviceResource
{
    private readonly VkImage _image;
    private readonly TextureType _type;
    private readonly PixelFormat _format;
    private readonly TextureUsage _usage;
    private readonly uint _width;
    private readonly uint _height;
    private readonly uint _mipLevels;
    private readonly uint _arrayLayers;
    private readonly DeviceMemory? _deviceMemory;
    private readonly bool isSwapchainImage;

    private ImageLayout _layout;

    internal Texture(GraphicsDevice graphicsDevice, in TextureDescription description) : base(graphicsDevice)
    {
        bool isCube = description.Usage.HasFlag(TextureUsage.Cubemap);

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
            MipLevels = description.MipLevels,
            ArrayLayers = isCube ? 6u : 1u,
            InitialLayout = ImageLayout.Preinitialized,
            Usage = Formats.GetImageUsageFlags(description.Usage),
            Tiling = ImageTiling.Optimal,
            Format = Formats.GetPixelFormat(description.Format, description.Usage.HasFlag(TextureUsage.DepthStencil)),
            Flags = ImageCreateFlags.CreateMutableFormatBit,
            Samples = Formats.GetSampleCount(description.SampleCount)
        };

        if (isCube)
        {
            imageCreateInfo.Flags |= ImageCreateFlags.CreateCubeCompatibleBit;
        }

        VkImage image;
        Vk.CreateImage(Device, &imageCreateInfo, null, &image);

        MemoryRequirements memoryRequirements;
        Vk.GetImageMemoryRequirements(Device, image, &memoryRequirements);

        bool isStaging = description.Usage.HasFlag(TextureUsage.Staging);

        DeviceMemory deviceMemory = new(graphicsDevice,
                                        memoryRequirements,
                                        isStaging ? MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit : MemoryPropertyFlags.DeviceLocalBit);

        Vk.BindImageMemory(Device, image, deviceMemory.Handle, 0);

        _image = image;
        _type = description.Type;
        _format = description.Format;
        _usage = description.Usage;
        _width = description.Width;
        _height = description.Height;
        _layout = ImageLayout.Preinitialized;
        _mipLevels = description.MipLevels;
        _arrayLayers = imageCreateInfo.ArrayLayers;
        _deviceMemory = deviceMemory;
        isSwapchainImage = false;
    }

    internal Texture(GraphicsDevice graphicsDevice, VkImage image, Format format, uint width, uint height) : base(graphicsDevice)
    {
        _image = image;
        _type = TextureType.Texture2D;
        _format = Formats.GetPixelFormat(format);
        _usage = TextureUsage.RenderTarget;
        _width = width;
        _height = height;
        _mipLevels = 1;
        _arrayLayers = 1;
        _deviceMemory = null;
        isSwapchainImage = true;
    }

    internal VkImage Handle => _image;

    internal TextureType Type => _type;

    internal PixelFormat Format => _format;

    internal TextureUsage Usage => _usage;

    internal uint Width => _width;

    internal uint Height => _height;

    internal uint MipLevels => _mipLevels;

    internal uint ArrayLayers => _arrayLayers;

    internal void TransitionImageLayout(ImageLayout newLayout)
    {
        CommandBuffer commandBuffer = GraphicsDevice.BeginSingleTimeCommands();

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
            }
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
            if ((_layout == ImageLayout.Undefined || _layout == ImageLayout.Preinitialized) && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.None;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                srcStageFlags = PipelineStageFlags.TopOfPipeBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.ShaderReadOnlyOptimal && newLayout == ImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                srcStageFlags = PipelineStageFlags.FragmentShaderBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.ShaderReadOnlyOptimal && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                srcStageFlags = PipelineStageFlags.FragmentShaderBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.Preinitialized && newLayout == ImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.None;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                srcStageFlags = PipelineStageFlags.TopOfPipeBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.Preinitialized && newLayout == ImageLayout.General)
            {
                barrier.SrcAccessMask = AccessFlags.None;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.TopOfPipeBit;
                dstStageFlags = PipelineStageFlags.ComputeShaderBit;
            }
            else if (_layout == ImageLayout.Preinitialized && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.None;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.TopOfPipeBit;
                dstStageFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else if (_layout == ImageLayout.General && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else if (_layout == ImageLayout.ShaderReadOnlyOptimal && newLayout == ImageLayout.General)
            {
                barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.FragmentShaderBit;
                dstStageFlags = PipelineStageFlags.ComputeShaderBit;
            }
            else if (_layout == ImageLayout.TransferSrcOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else if (_layout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else if (_layout == ImageLayout.TransferSrcOptimal && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.ColorAttachmentOptimal && newLayout == ImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                srcStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.ColorAttachmentOptimal && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                srcStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.ColorAttachmentOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else if (_layout == ImageLayout.DepthStencilAttachmentOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;
                srcStageFlags = PipelineStageFlags.LateFragmentTestsBit;
                dstStageFlags = PipelineStageFlags.FragmentShaderBit;
            }
            else if (_layout == ImageLayout.ColorAttachmentOptimal && newLayout == ImageLayout.PresentSrcKhr)
            {
                barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = AccessFlags.MemoryReadBit;
                srcStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = PipelineStageFlags.BottomOfPipeBit;
            }
            else if (_layout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.PresentSrcKhr)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.MemoryReadBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.BottomOfPipeBit;
            }
            else if (_layout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ColorAttachmentOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ColorAttachmentWriteBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;
            }
            else if (_layout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.DepthStencilAttachmentWriteBit;
                srcStageFlags = PipelineStageFlags.TransferBit;
                dstStageFlags = PipelineStageFlags.LateFragmentTestsBit;
            }
            else if (_layout == ImageLayout.General && newLayout == ImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ShaderWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                srcStageFlags = PipelineStageFlags.ComputeShaderBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.General && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.ShaderWriteBit;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                srcStageFlags = PipelineStageFlags.ComputeShaderBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else if (_layout == ImageLayout.PresentSrcKhr && newLayout == ImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.MemoryReadBit;
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                srcStageFlags = PipelineStageFlags.BottomOfPipeBit;
                dstStageFlags = PipelineStageFlags.TransferBit;
            }
            else
            {
                throw new InvalidOperationException("Unsupported layout transition!");
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

        GraphicsDevice.EndSingleTimeCommands(commandBuffer);

        _layout = newLayout;
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
        if (!isSwapchainImage)
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
