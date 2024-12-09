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
        ColorTargets = new TextureView[desc.ColorTargets.Length];

        TextureSampleCount sampleCount = TextureSampleCount.Count1;

        RenderingAttachmentInfo* colorAttachmentInfos = Allocator.Alloc<RenderingAttachmentInfo>((uint)ColorTargets.Length);
        RenderingAttachmentInfo* depthStencilAttachmentInfo = null;

        PixelFormat[] colorFormats = new PixelFormat[ColorTargets.Length];
        PixelFormat? depthStencilFormat = null;

        for (int i = 0; i < ColorTargets.Length; i++)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.ColorTargets[i];
            Texture target = attachmentDesc.Target;

            if (i is 0)
            {
                sampleCount = target.Desc.SampleCount;
            }
            else if (target.Desc.SampleCount != sampleCount)
            {
                throw new ZenithEngineException("All targets must have the same sample count.");
            }

            TextureViewDesc viewDesc = new()
            {
                Target = target,
                Format = target.Desc.Format,
                BaseMipLevel = attachmentDesc.MipLevel,
                MipLevels = 1,
                BaseFace = attachmentDesc.Face,
                FaceCount = 1
            };

            ColorTargets[i] = Context.Factory.CreateTextureView(in viewDesc);

            colorAttachmentInfos[i] = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = ColorTargets[i].VK().ImageView,
                ImageLayout = ImageLayout.AttachmentOptimal,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store
            };

            colorFormats[i] = viewDesc.Format;
        }

        if (desc.DepthStencilTarget.HasValue)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.DepthStencilTarget.Value;
            Texture target = attachmentDesc.Target;

            if (ColorTargets.Length is 0)
            {
                sampleCount = target.Desc.SampleCount;
            }
            else if (target.Desc.SampleCount != sampleCount)
            {
                throw new ZenithEngineException("All targets must have the same sample count.");
            }

            TextureViewDesc viewDesc = new()
            {
                Target = target,
                Format = target.Desc.Format,
                BaseMipLevel = attachmentDesc.MipLevel,
                MipLevels = 1,
                BaseFace = attachmentDesc.Face,
                FaceCount = 1
            };

            DepthStencilTarget = Context.Factory.CreateTextureView(in viewDesc);

            depthStencilAttachmentInfo = Allocator.Alloc<RenderingAttachmentInfo>();
            depthStencilAttachmentInfo->SType = StructureType.RenderingAttachmentInfo;
            depthStencilAttachmentInfo->ImageView = DepthStencilTarget!.VK().ImageView;
            depthStencilAttachmentInfo->ImageLayout = ImageLayout.AttachmentOptimal;
            depthStencilAttachmentInfo->LoadOp = AttachmentLoadOp.Load;
            depthStencilAttachmentInfo->StoreOp = AttachmentStoreOp.Store;

            depthStencilFormat = viewDesc.Format;
        }

        TextureView view = ColorTargets.Length > 0 ? ColorTargets[0] : DepthStencilTarget!;

        Utils.GetMipDimensions(view.Desc.Target.Desc.Width,
                               view.Desc.Target.Desc.Height,
                               view.Desc.BaseMipLevel,
                               out uint width,
                               out uint height);

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
            ColorAttachmentCount = (uint)ColorTargets.Length,
            PColorAttachments = colorAttachmentInfos,
            PDepthAttachment = depthStencilAttachmentInfo,
            PStencilAttachment = depthStencilAttachmentInfo
        };

        Width = width;
        Height = height;
        Output = OutputDesc.Default(sampleCount, depthStencilFormat, colorFormats);
    }

    public TextureView[] ColorTargets { get; }

    public TextureView? DepthStencilTarget { get; }

    public override uint Width { get; }

    public override uint Height { get; }

    public override OutputDesc Output { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    public void TransitionToIntermedialLayout(VkCommandBuffer commandBuffer)
    {
        foreach (TextureView colorTarget in ColorTargets)
        {
            colorTarget.VK().TransitionLayout(commandBuffer, ImageLayout.ColorAttachmentOptimal);
        }

        DepthStencilTarget?.VK().TransitionLayout(commandBuffer, ImageLayout.DepthStencilAttachmentOptimal);
    }

    public void TransitionToFinalLayout(VkCommandBuffer commandBuffer)
    {
        foreach (TextureView colorTarget in ColorTargets)
        {
            Texture texture = colorTarget.Desc.Target;

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
                colorTarget.VK().TransitionLayout(commandBuffer, imageLayout);
            }
        }

        if (DepthStencilTarget is not null)
        {
            if (DepthStencilTarget.Desc.Target.Desc.Usage.HasFlag(TextureUsage.Sampled))
            {
                DepthStencilTarget.VK().TransitionLayout(commandBuffer, ImageLayout.ShaderReadOnlyOptimal);
            }
        }
    }

    protected override void DebugName(string name)
    {
        for (int i = 0; i < ColorTargets.Length; i++)
        {
            ColorTargets[i].Name = $"{name} Color Target[{i}]";
        }

        if (DepthStencilTarget is not null)
        {
            DepthStencilTarget.Name = $"{name} Depth Stencil Target";
        }
    }

    protected override void Destroy()
    {
        foreach (TextureView colorTarget in ColorTargets)
        {
            colorTarget.Dispose();
        }

        DepthStencilTarget?.Dispose();
    }
}
