using Graphics.Engine.Descriptions;
using Graphics.Engine.Helpers;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKFrameBuffer : FrameBuffer
{
    public VKFrameBuffer(Context context,
                         ref readonly FrameBufferDescription description) : base(context, in description)
    {
        bool hasDepthStencil = description.DepthStencilTarget.HasValue;

        ColorTargets = new TextureView[description.ColorTargets.Length];

        for (int i = 0; i < ColorTargets.Length; i++)
        {
            FrameBufferAttachmentDescription attachmentDescription = description.ColorTargets[i];

            TextureViewDescription textureViewDescription = new()
            {
                Target = attachmentDescription.Target,
                BaseFace = attachmentDescription.Face,
                FaceCount = 1,
                BaseMipLevel = attachmentDescription.MipLevel,
                MipLevels = 1
            };

            ColorTargets[i] = context.Factory.CreateTextureView(in textureViewDescription);
        }

        if (hasDepthStencil)
        {
            FrameBufferAttachmentDescription attachmentDescription = description.DepthStencilTarget!.Value;

            TextureViewDescription textureViewDescription = new()
            {
                Target = attachmentDescription.Target,
                BaseFace = attachmentDescription.Face,
                FaceCount = 1,
                BaseMipLevel = attachmentDescription.MipLevel,
                MipLevels = 1
            };

            DepthStencilTarget = context.Factory.CreateTextureView(in textureViewDescription);
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

        Utils.GetMipDimensions(view.Description.Target.Description.Width,
                               view.Description.Target.Description.Height,
                               view.Description.BaseMipLevel,
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
