using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKFrameBuffer : FrameBuffer
{
    public VKFrameBuffer(Context context,
                         ref readonly FrameBufferDescription description) : base(context, in description)
    {
        ColorTargets = new TextureView[description.ColorTargets.Length];

        for (int i = 0; i < description.ColorTargets.Length; i++)
        {
            FrameBufferAttachmentDescription attachmentDescription = description.ColorTargets[i];

            TextureViewDescription textureViewDescription = new(attachmentDescription.Target,
                                                                attachmentDescription.Face,
                                                                1,
                                                                attachmentDescription.MipLevel,
                                                                1);

            ColorTargets[i] = context.Factory.CreateTextureView(in textureViewDescription);

            Width = ColorTargets[i].Width;
            Height = ColorTargets[i].Height;
        }

        if (description.DepthStencilTarget.HasValue)
        {
            FrameBufferAttachmentDescription attachmentDescription = description.DepthStencilTarget.Value;

            TextureViewDescription textureViewDescription = new(attachmentDescription.Target,
                                                                attachmentDescription.Face,
                                                                1,
                                                                attachmentDescription.MipLevel,
                                                                1);

            DepthStencilTarget = context.Factory.CreateTextureView(in textureViewDescription);

            Width = DepthStencilTarget.Width;
            Height = DepthStencilTarget.Height;
        }

        RenderingAttachmentInfo* colorAttachmentInfos = Allocator.Alloc<RenderingAttachmentInfo>(ColorTargets.Length);
        RenderingAttachmentInfo* depthStencilAttachmentInfo = null;

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

        if (DepthStencilTarget != null)
        {
            depthStencilAttachmentInfo = Allocator.Alloc<RenderingAttachmentInfo>();

            depthStencilAttachmentInfo[0] = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = DepthStencilTarget.VK().ImageView,
                ImageLayout = ImageLayout.AttachmentOptimal,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store
            };
        }

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
                    Width = Width,
                    Height = Height
                }
            },
            LayerCount = 1,
            ViewMask = 0,
            ColorAttachmentCount = (uint)ColorTargets.Length,
            PColorAttachments = colorAttachmentInfos,
            PDepthAttachment = depthStencilAttachmentInfo,
            PStencilAttachment = depthStencilAttachmentInfo
        };
    }

    public new VKContext Context => (VKContext)base.Context;

    public override uint Width { get; }

    public override uint Height { get; }

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

        base.Destroy();
    }
}
