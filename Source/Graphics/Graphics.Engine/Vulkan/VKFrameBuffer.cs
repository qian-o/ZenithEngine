using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed class VKFrameBuffer : FrameBuffer
{
    public VKFrameBuffer(Context context, ref readonly FrameBufferDescription description) : base(context, in description)
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
        }

        ColorAttachmentInfos = new RenderingAttachmentInfo[ColorTargets.Length];

        for (int i = 0; i < ColorTargets.Length; i++)
        {
            ColorAttachmentInfos[i] = new()
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
            DepthStencilAttachmentInfo = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = DepthStencilTarget.VK().ImageView,
                ImageLayout = ImageLayout.AttachmentOptimal,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store
            };
        }
    }

    public new VKContext Context => (VKContext)base.Context;

    public TextureView[] ColorTargets { get; }

    public TextureView? DepthStencilTarget { get; }

    public RenderingAttachmentInfo[] ColorAttachmentInfos { get; }

    public RenderingAttachmentInfo? DepthStencilAttachmentInfo { get; }

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
