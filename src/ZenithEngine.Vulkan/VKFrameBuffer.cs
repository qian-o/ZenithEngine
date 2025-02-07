using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKFrameBuffer : FrameBuffer
{
    public RenderingInfo RenderingInfo;

    private readonly VkImageView[] colorViews;
    private readonly VkImageView depthStencilView;

    public VKFrameBuffer(GraphicsContext context,
                         ref readonly FrameBufferDesc desc) : base(context, in desc)
    {
        ColorAttachmentCount = (uint)desc.ColorTargets.Length;
        HasDepthStencilAttachment = desc.DepthStencilTarget.HasValue;

        colorViews = new VkImageView[ColorAttachmentCount];

        uint width = 0;
        uint height = 0;
        TextureSampleCount sampleCount = TextureSampleCount.Count1;

        RenderingAttachmentInfo* colorAttachments = Allocator.Alloc<RenderingAttachmentInfo>(ColorAttachmentCount);
        RenderingAttachmentInfo* depthStencilAttachment = HasDepthStencilAttachment ? Allocator.Alloc<RenderingAttachmentInfo>() : null;

        for (uint i = 0; i < ColorAttachmentCount; i++)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.ColorTargets[i];
            Texture target = attachmentDesc.Target;

            if (i is 0)
            {
                Utils.GetMipDimensions(target.Desc.Width,
                                       target.Desc.Height,
                                       attachmentDesc.MipLevel,
                                       out width,
                                       out height);

                sampleCount = target.Desc.SampleCount;
            }
            else if (target.Desc.SampleCount != sampleCount)
            {
                throw new ZenithEngineException("All targets must have the same sample count.");
            }

            VkImageView imageView = CreateImageView(target.VK(),
                                                    attachmentDesc.MipLevel,
                                                    attachmentDesc.ArrayLayer,
                                                    attachmentDesc.Face);

            colorAttachments[i] = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = colorViews[i] = imageView,
                ImageLayout = ImageLayout.AttachmentOptimal,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store
            };
        }

        if (HasDepthStencilAttachment)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.DepthStencilTarget!.Value;
            Texture target = attachmentDesc.Target;

            if (ColorAttachmentCount is 0)
            {
                Utils.GetMipDimensions(target.Desc.Width,
                                       target.Desc.Height,
                                       attachmentDesc.MipLevel,
                                       out width,
                                       out height);

                sampleCount = target.Desc.SampleCount;
            }
            else if (target.Desc.SampleCount != sampleCount)
            {
                throw new ZenithEngineException("All targets must have the same sample count.");
            }

            VkImageView imageView = CreateImageView(target.VK(),
                                                    attachmentDesc.MipLevel,
                                                    attachmentDesc.ArrayLayer,
                                                    attachmentDesc.Face);

            depthStencilAttachment[0] = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = depthStencilView = imageView,
                ImageLayout = ImageLayout.AttachmentOptimal,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store
            };
        }

        RenderingInfo = new()
        {
            SType = StructureType.RenderingInfo,
            RenderArea = new()
            {
                Offset = new()
                {
                    X = 0,
                    Y = 0
                },
                Extent = new()
                {
                    Width = width,
                    Height = height
                }
            },
            LayerCount = 1,
            ViewMask = 0,
            ColorAttachmentCount = ColorAttachmentCount,
            PColorAttachments = colorAttachments,
            PDepthAttachment = depthStencilAttachment,
            PStencilAttachment = depthStencilAttachment
        };

        Width = width;
        Height = height;
        Output = OutputDesc.New(sampleCount,
                                HasDepthStencilAttachment ? desc.DepthStencilTarget!.Value.Target.Desc.Format : null,
                                [.. desc.ColorTargets.Select(static item => item.Target.Desc.Format)]);
    }

    public override uint ColorAttachmentCount { get; }

    public override bool HasDepthStencilAttachment { get; }

    public override uint Width { get; }

    public override uint Height { get; }

    public override OutputDesc Output { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void TransitionToIntermediateLayout(VkCommandBuffer commandBuffer)
    {
        foreach (FrameBufferAttachmentDesc desc in Desc.ColorTargets)
        {
            desc.Target.VK().TransitionLayout(commandBuffer,
                                              desc.MipLevel,
                                              1,
                                              desc.ArrayLayer,
                                              1,
                                              desc.Face,
                                              1,
                                              ImageLayout.ColorAttachmentOptimal);
        }

        if (Desc.DepthStencilTarget is not null)
        {
            FrameBufferAttachmentDesc desc = Desc.DepthStencilTarget.Value;

            desc.Target.VK().TransitionLayout(commandBuffer,
                                              desc.MipLevel,
                                              1,
                                              desc.ArrayLayer,
                                              1,
                                              desc.Face,
                                              1,
                                              ImageLayout.DepthStencilAttachmentOptimal);
        }
    }

    public void TransitionToFinalLayout(VkCommandBuffer commandBuffer)
    {
        foreach (FrameBufferAttachmentDesc desc in Desc.ColorTargets)
        {
            VKTexture texture = desc.Target.VK();

            ImageLayout layout;
            if (texture.Desc.Usage.HasFlag(TextureUsage.Sampled))
            {
                layout = ImageLayout.ShaderReadOnlyOptimal;
            }
            else if (texture.Desc.Usage.HasFlag(TextureUsage.Storage))
            {
                layout = ImageLayout.General;
            }
            else if (texture.Desc.Usage.HasFlag(TextureUsage.RenderTarget))
            {
                layout = ImageLayout.PresentSrcKhr;
            }
            else
            {
                continue;
            }

            texture.TransitionLayout(commandBuffer,
                                     desc.MipLevel,
                                     1,
                                     desc.ArrayLayer,
                                     1,
                                     desc.Face,
                                     1,
                                     layout);
        }

        if (Desc.DepthStencilTarget is not null)
        {
            FrameBufferAttachmentDesc desc = Desc.DepthStencilTarget.Value;

            VKTexture texture = desc.Target.VK();

            if (texture.Desc.Usage.HasFlag(TextureUsage.Sampled))
            {
                texture.TransitionLayout(commandBuffer,
                                         desc.MipLevel,
                                         1,
                                         desc.ArrayLayer,
                                         1,
                                         desc.Face,
                                         1,
                                         ImageLayout.ShaderReadOnlyOptimal);
            }
        }
    }

    protected override void DebugName(string name)
    {
        for (int i = 0; i < ColorAttachmentCount; i++)
        {
            Context.SetDebugName(ObjectType.ImageView, colorViews[i].Handle, $"{name} Color Target[{i}]");
        }

        if (HasDepthStencilAttachment)
        {
            Context.SetDebugName(ObjectType.ImageView, depthStencilView.Handle, $"{name} Depth Stencil Target");
        }
    }

    protected override void Destroy()
    {
        for (int i = 0; i < ColorAttachmentCount; i++)
        {
            Context.Vk.DestroyImageView(Context.Device, colorViews[i], null);
        }

        if (HasDepthStencilAttachment)
        {
            Context.Vk.DestroyImageView(Context.Device, depthStencilView, null);
        }
    }

    private VkImageView CreateImageView(VKTexture texture,
                                        uint mipLevel,
                                        uint arrayLayer,
                                        CubeMapFace face)
    {
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = texture.Image,
            ViewType = ImageViewType.Type2D,
            Format = VKFormats.GetPixelFormat(texture.Desc.Format),
            SubresourceRange = new()
            {
                AspectMask = VKFormats.GetImageAspectFlags(texture.Desc.Usage),
                BaseMipLevel = mipLevel,
                LevelCount = 1,
                BaseArrayLayer = VKHelpers.GetArrayLayerIndex(texture.Desc, mipLevel, arrayLayer, face),
                LayerCount = 1
            }
        };

        VkImageView imageView;
        Context.Vk.CreateImageView(Context.Device, &createInfo, null, &imageView).ThrowIfError();

        return imageView;
    }
}
