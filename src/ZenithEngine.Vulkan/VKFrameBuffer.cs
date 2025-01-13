using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKFrameBuffer : FrameBuffer
{
    public RenderingInfo RenderingInfo;

    public VKFrameBuffer(GraphicsContext context,
                         ref readonly FrameBufferDesc desc) : base(context, in desc)
    {
        uint colorAttachmentCount = (uint)desc.ColorTargets.Length;
        bool hasDepthStencil = desc.DepthStencilTarget.HasValue;

        ColorViews = new VkImageView[colorAttachmentCount];

        uint width = 0;
        uint height = 0;
        TextureSampleCount sampleCount = TextureSampleCount.Count1;

        RenderingAttachmentInfo* colorAttachments = Allocator.Alloc<RenderingAttachmentInfo>(colorAttachmentCount);
        RenderingAttachmentInfo* depthStencilAttachment = hasDepthStencil ? Allocator.Alloc<RenderingAttachmentInfo>() : null;

        for (uint i = 0; i < colorAttachmentCount; i++)
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
                throw new Exception("All targets must have the same sample count.");
            }

            VkImageView imageView = target.VK().CreateImageView(attachmentDesc.MipLevel,
                                                                1,
                                                                attachmentDesc.Face,
                                                                1);

            colorAttachments[i] = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = ColorViews[i] = imageView,
                ImageLayout = ImageLayout.AttachmentOptimal,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store
            };
        }

        if (hasDepthStencil)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.DepthStencilTarget!.Value;
            Texture target = attachmentDesc.Target;

            if (colorAttachmentCount is 0)
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
                throw new Exception("All targets must have the same sample count.");
            }

            VkImageView imageView = target.VK().CreateImageView(attachmentDesc.MipLevel,
                                                                1,
                                                                attachmentDesc.Face,
                                                                1);

            depthStencilAttachment[0] = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = (DepthStencilView = imageView).Value,
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
            ColorAttachmentCount = colorAttachmentCount,
            PColorAttachments = colorAttachments,
            PDepthAttachment = depthStencilAttachment,
            PStencilAttachment = depthStencilAttachment
        };

        Width = width;
        Height = height;
        Output = OutputDesc.Default(sampleCount,
                                    hasDepthStencil ? desc.DepthStencilTarget!.Value.Target.Desc.Format : null,
                                    [.. desc.ColorTargets.Select(static x => x.Target.Desc.Format)]);
    }

    public VkImageView[] ColorViews { get; }

    public VkImageView? DepthStencilView { get; }

    public override uint Width { get; }

    public override uint Height { get; }

    public override OutputDesc Output { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void TransitionToIntermedialLayout(VkCommandBuffer commandBuffer)
    {
        foreach (FrameBufferAttachmentDesc desc in Desc.ColorTargets)
        {
            desc.Target.VK().TransitionLayout(commandBuffer,
                                              desc.MipLevel,
                                              1,
                                              desc.Face,
                                              1,
                                              ImageLayout.ColorAttachmentOptimal);
        }

        if (Desc.DepthStencilTarget is not null)
        {
            FrameBufferAttachmentDesc desc = Desc.DepthStencilTarget!.Value;

            desc.Target.VK().TransitionLayout(commandBuffer,
                                              desc.MipLevel,
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

            ImageLayout imageLayout = ImageLayout.Undefined;

            if (texture.Desc.Usage.HasFlag(TextureUsage.Sampled))
            {
                imageLayout = ImageLayout.ShaderReadOnlyOptimal;
            }
            else if (texture.Desc.Usage.HasFlag(TextureUsage.RenderTarget))
            {
                imageLayout = ImageLayout.PresentSrcKhr;
            }

            if (imageLayout is not ImageLayout.Undefined)
            {
                texture.TransitionLayout(commandBuffer,
                                         desc.MipLevel,
                                         1,
                                         desc.Face,
                                         1,
                                         imageLayout);
            }
        }

        if (Desc.DepthStencilTarget is not null)
        {
            FrameBufferAttachmentDesc desc = Desc.DepthStencilTarget!.Value;

            VKTexture texture = desc.Target.VK();

            if (texture.Desc.Usage.HasFlag(TextureUsage.Sampled))
            {
                texture.TransitionLayout(commandBuffer,
                                         desc.MipLevel,
                                         1,
                                         desc.Face,
                                         1,
                                         ImageLayout.ShaderReadOnlyOptimal);
            }
        }
    }

    protected override void DebugName(string name)
    {
        for (int i = 0; i < ColorViews.Length; i++)
        {
            Context.SetDebugName(ObjectType.ImageView, ColorViews[i].Handle, $"{name} Color Target[{i}]");
        }

        if (DepthStencilView is not null)
        {
            Context.SetDebugName(ObjectType.ImageView, DepthStencilView.Value.Handle, $"{name} Depth Stencil Target");
        }
    }

    protected override void Destroy()
    {
        for (int i = 0; i < ColorViews.Length; i++)
        {
            Context.Vk.DestroyImageView(Context.Device, ColorViews[i], null);
        }

        if (DepthStencilView is not null)
        {
            Context.Vk.DestroyImageView(Context.Device, DepthStencilView.Value, null);
        }
    }
}
