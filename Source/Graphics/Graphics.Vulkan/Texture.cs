using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Texture : DeviceResource, IBindableResource
{
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

        DeviceMemory deviceMemory = new(graphicsDevice,
                                        in memoryRequirements,
                                        MemoryPropertyFlags.DeviceLocalBit,
                                        false);

        Vk.BindImageMemory(Device, image, deviceMemory.Handle, 0).ThrowCode();

        ImageLayout[] imageLayouts = new ImageLayout[subresourceCount];
        Array.Fill(imageLayouts, ImageLayout.Preinitialized);

        Handle = image;
        Type = description.Type;
        Format = description.Format;
        VkFormat = createInfo.Format;
        SampleCount = description.SampleCount;
        VkSampleCount = createInfo.Samples;
        Usage = description.Usage;
        Width = description.Width;
        Height = description.Height;
        Depth = description.Depth;
        MipLevels = description.MipLevels;
        ArrayLayers = createInfo.ArrayLayers;
        _imageLayouts = imageLayouts;
        _deviceMemory = deviceMemory;
        _isSwapchainImage = false;

        TransitionToBestLayout();
    }

    internal Texture(GraphicsDevice graphicsDevice, VkImage image, Format format, uint width, uint height) : base(graphicsDevice)
    {
        Handle = image;
        Type = TextureType.Texture2D;
        Format = Formats.GetPixelFormat(format);
        VkFormat = format;
        SampleCount = TextureSampleCount.Count1;
        VkSampleCount = SampleCountFlags.Count1Bit;
        Usage = TextureUsage.RenderTarget;
        Width = width;
        Height = height;
        Depth = 1;
        MipLevels = 1;
        ArrayLayers = 1;
        _imageLayouts = [ImageLayout.Undefined];
        _deviceMemory = null;
        _isSwapchainImage = true;
    }

    internal VkImage Handle { get; }

    internal TextureType Type { get; }

    internal PixelFormat Format { get; }

    internal Format VkFormat { get; }

    internal TextureSampleCount SampleCount { get; }

    internal SampleCountFlags VkSampleCount { get; }

    internal TextureUsage Usage { get; }

    public uint Width { get; }

    public uint Height { get; }

    public uint Depth { get; }

    public uint MipLevels { get; }

    public uint ArrayLayers { get; }

    internal void TransitionLayout(CommandBuffer commandBuffer, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ImageLayout newLayout)
    {
        for (uint level = baseMipLevel; level < baseMipLevel + levelCount; level++)
        {
            for (uint layer = baseArrayLayer; layer < baseArrayLayer + layerCount; layer++)
            {
                uint index = (layer * MipLevels) + level;

                ImageLayout oldLayout = _imageLayouts[index];

                if (oldLayout != newLayout)
                {
                    ImageMemoryBarrier barrier = new()
                    {
                        SType = StructureType.ImageMemoryBarrier,
                        Image = Handle,
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

                        if (HasStencilComponent(Format))
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
        TransitionLayout(commandBuffer, 0, MipLevels, 0, ArrayLayers, newLayout);
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

    internal void TransitionToBestLayout()
    {
        using StagingCommandPool stagingCommandPool = new(GraphicsDevice, TransferQueue);

        CommandBuffer commandBuffer = stagingCommandPool.BeginNewCommandBuffer();

        TransitionToBestLayout(commandBuffer);

        stagingCommandPool.EndAndSubmitCommandBuffer(commandBuffer);
    }

    protected override void Destroy()
    {
        if (!_isSwapchainImage)
        {
            Vk.DestroyImage(Device, Handle, null);

            _deviceMemory!.Dispose();
        }
    }

    private static bool HasStencilComponent(PixelFormat format)
    {
        return format is PixelFormat.D24UNormS8UInt or PixelFormat.D32FloatS8UInt;
    }
}
