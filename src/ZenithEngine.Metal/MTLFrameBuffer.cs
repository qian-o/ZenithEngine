using System;
using System.Linq;
using SharpMetal.Metal;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLFrameBuffer : FrameBuffer
{
    public MTLFrameBuffer(GraphicsContext context,
                          ref readonly FrameBufferDesc desc) : base(context, in desc)
    {
        ColorAttachmentCount = (uint)desc.ColorTargets.Length;
        HasDepthStencilAttachment = desc.DepthStencilTarget.HasValue;

        ColorAttachments = new FrameBufferAttachmentDesc[ColorAttachmentCount];
        
        uint width = 0;
        uint height = 0;
        TextureSampleCount sampleCount = TextureSampleCount.Count1;

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

            ColorAttachments[i] = attachmentDesc;
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

            DepthStencilAttachment = attachmentDesc;
        }

        Width = width;
        Height = height;
        Output = new(sampleCount,
                     [.. desc.ColorTargets.Select(static item => new OutputAttachmentDesc(item.Target.Desc.Format))],
                     desc.DepthStencilTarget.HasValue ? new OutputAttachmentDesc(desc.DepthStencilTarget.Value.Target.Desc.Format) : null,
                     null);
    }

    public FrameBufferAttachmentDesc[] ColorAttachments { get; }

    public FrameBufferAttachmentDesc? DepthStencilAttachment { get; }

    public override uint ColorAttachmentCount { get; }

    public override bool HasDepthStencilAttachment { get; }

    public override uint Width { get; }

    public override uint Height { get; }

    public override OutputDesc Output { get; }

    /// <summary>
    /// Creates a Metal render pass descriptor from this framebuffer.
    /// </summary>
    /// <param name="clearValue">Clear value for the attachments.</param>
    /// <returns>A configured MTLRenderPassDescriptor.</returns>
    public SharpMetal.Metal.MTLRenderPassDescriptor CreateRenderPassDescriptor(ClearValue clearValue)
    {
        MTLRenderPassDescriptor passDescriptor = MTLRenderPassDescriptor.Create();

        // Configure color attachments
        for (nuint i = 0; i < ColorAttachmentCount; i++)
        {
            FrameBufferAttachmentDesc attachmentDesc = ColorAttachments[i];
            MTLTexture texture = (MTLTexture)attachmentDesc.Target;

            MTLRenderPassColorAttachmentDescriptor colorAttachment = passDescriptor.GetColorAttachment(i);
            colorAttachment.Texture = texture.Texture;
            colorAttachment.LoadAction = MTLLoadAction.Clear;
            colorAttachment.StoreAction = MTLStoreAction.Store;
            colorAttachment.ClearColor = new MTLClearColor(
                clearValue.Color.X,
                clearValue.Color.Y,
                clearValue.Color.Z,
                clearValue.Color.W);

            if (attachmentDesc.MipLevel > 0)
            {
                colorAttachment.Level = attachmentDesc.MipLevel;
            }

            if (attachmentDesc.ArrayLayer > 0)
            {
                colorAttachment.Slice = attachmentDesc.ArrayLayer;
            }
        }

        // Configure depth/stencil attachment
        if (HasDepthStencilAttachment)
        {
            FrameBufferAttachmentDesc attachmentDesc = DepthStencilAttachment!.Value;
            MTLTexture texture = (MTLTexture)attachmentDesc.Target;

            MTLRenderPassDepthAttachmentDescriptor depthAttachment = passDescriptor.DepthAttachment;
            depthAttachment.Texture = texture.Texture;
            depthAttachment.LoadAction = MTLLoadAction.Clear;
            depthAttachment.StoreAction = MTLStoreAction.Store;
            depthAttachment.ClearDepth = clearValue.DepthStencil.X;

            if (attachmentDesc.MipLevel > 0)
            {
                depthAttachment.Level = attachmentDesc.MipLevel;
            }

            if (attachmentDesc.ArrayLayer > 0)
            {
                depthAttachment.Slice = attachmentDesc.ArrayLayer;
            }

            // Check if format has stencil component
            PixelFormat format = texture.Desc.Format;
            if (format == PixelFormat.D24UNormS8UInt || format == PixelFormat.D32FloatS8UInt)
            {
                MTLRenderPassStencilAttachmentDescriptor stencilAttachment = passDescriptor.StencilAttachment;
                stencilAttachment.Texture = texture.Texture;
                stencilAttachment.LoadAction = MTLLoadAction.Clear;
                stencilAttachment.StoreAction = MTLStoreAction.Store;
                stencilAttachment.ClearStencil = (uint)clearValue.DepthStencil.Y;

                if (attachmentDesc.MipLevel > 0)
                {
                    stencilAttachment.Level = attachmentDesc.MipLevel;
                }

                if (attachmentDesc.ArrayLayer > 0)
                {
                    stencilAttachment.Slice = attachmentDesc.ArrayLayer;
                }
            }
        }

        return passDescriptor;
    }

    protected override void SetName(string name)
    {
        // Framebuffers don't have names in Metal, they're just descriptors
    }

    protected override void Destroy()
    {
        // Nothing to dispose for framebuffers - they're just descriptor objects
    }
}
