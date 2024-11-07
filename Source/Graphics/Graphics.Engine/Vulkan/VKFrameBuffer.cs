﻿using Graphics.Engine.Descriptions;
using Graphics.Engine.Helpers;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKFrameBuffer : FrameBuffer
{
    public VKFrameBuffer(Context context,
                         ref readonly FrameBufferDesc desc) : base(context, in desc)
    {
        bool hasDepthStencil = desc.DepthStencilTarget.HasValue;

        ColorTargets = new TextureView[desc.ColorTargets.Length];

        for (int i = 0; i < ColorTargets.Length; i++)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.ColorTargets[i];

            TextureViewDesc textureViewDesc = new()
            {
                Target = attachmentDesc.Target,
                BaseFace = attachmentDesc.Face,
                FaceCount = 1,
                BaseMipLevel = attachmentDesc.MipLevel,
                MipLevels = 1
            };

            ColorTargets[i] = context.Factory.CreateTextureView(in textureViewDesc);
        }

        if (hasDepthStencil)
        {
            FrameBufferAttachmentDesc attachmentDesc = desc.DepthStencilTarget!.Value;

            TextureViewDesc textureViewDesc = new()
            {
                Target = attachmentDesc.Target,
                BaseFace = attachmentDesc.Face,
                FaceCount = 1,
                BaseMipLevel = attachmentDesc.MipLevel,
                MipLevels = 1
            };

            DepthStencilTarget = context.Factory.CreateTextureView(in textureViewDesc);
        }

        RenderingAttachmentInfo[] colorAttachmentInfos = new RenderingAttachmentInfo[ColorTargets.Length];

        for (int i = 0; i < ColorTargets.Length; i++)
        {
            colorAttachmentInfos[i] = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = ColorTargets[i].VK().ImageView,
                ImageLayout = ImageLayout.AttachmentOptimal,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store
            };
        }

        RenderingAttachmentInfo? depthStencilAttachmentInfo = hasDepthStencil ? new()
        {
            SType = StructureType.RenderingAttachmentInfo,
            ImageView = DepthStencilTarget!.VK().ImageView,
            ImageLayout = ImageLayout.AttachmentOptimal,
            LoadOp = AttachmentLoadOp.Load,
            StoreOp = AttachmentStoreOp.Store
        } : null;

        TextureView view = ColorTargets.Length > 0 ? ColorTargets[0] : DepthStencilTarget!;

        Utils.GetMipDimensions(view.Desc.Target.Desc.Width,
                               view.Desc.Target.Desc.Height,
                               view.Desc.BaseMipLevel,
                               out uint width,
                               out uint height);

        RenderingInfo = new()
        {
            SType = StructureType.RenderingInfo,
            RenderArea = new Rect2D
            {
                Offset = new Offset2D
                {
                    X = 0,
                    Y = 0
                },
                Extent = new Extent2D
                {
                    Width = width,
                    Height = height
                }
            },
            LayerCount = 1,
            ViewMask = 0,
            ColorAttachmentCount = (uint)ColorTargets.Length,
            PColorAttachments = Allocator.Alloc(colorAttachmentInfos),
            PDepthAttachment = hasDepthStencil ? Allocator.Alloc(depthStencilAttachmentInfo!.Value) : null,
            PStencilAttachment = hasDepthStencil ? Allocator.Alloc(depthStencilAttachmentInfo!.Value) : null
        };
    }

    public new VKContext Context => (VKContext)base.Context;

    public TextureView[] ColorTargets { get; }

    public TextureView? DepthStencilTarget { get; }

    public RenderingInfo RenderingInfo { get; }

    protected override void SetName(string name)
    {
        for (int i = 0; i < ColorTargets.Length; i++)
        {
            ColorTargets[i].Name = $"{name} Color Target[{i}]";
        }

        if (DepthStencilTarget != null)
        {
            DepthStencilTarget.Name = $"{name} Depth Stencil Target";
        }
    }

    protected override void Destroy()
    {
        foreach (TextureView textureView in ColorTargets)
        {
            textureView.Dispose();
        }

        DepthStencilTarget?.Dispose();
    }
}
