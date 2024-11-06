using Graphics.Engine.Descriptions;

namespace Graphics.Engine.Vulkan;

internal sealed class VKFrameBuffer : FrameBuffer
{
    public VKFrameBuffer(Context context, ref readonly FrameBufferDescription description) : base(context, in description)
    {
        TextureView[] colorTargets = new TextureView[description.ColorTargets.Length];
        TextureView? depthStencilTarget = null;

        for (int i = 0; i < description.ColorTargets.Length; i++)
        {
            FrameBufferAttachmentDescription attachmentDescription = description.ColorTargets[i];

            TextureViewDescription textureViewDescription = new(attachmentDescription.Target,
                                                                attachmentDescription.Face,
                                                                1,
                                                                attachmentDescription.MipLevel,
                                                                1);

            colorTargets[i] = context.Factory.CreateTextureView(in textureViewDescription);
        }

        if (description.DepthStencilTarget.HasValue)
        {
            FrameBufferAttachmentDescription attachmentDescription = description.DepthStencilTarget.Value;

            TextureViewDescription textureViewDescription = new(attachmentDescription.Target,
                                                                attachmentDescription.Face,
                                                                1,
                                                                attachmentDescription.MipLevel,
                                                                1);

            depthStencilTarget = context.Factory.CreateTextureView(in textureViewDescription);
        }

        ColorTargets = colorTargets;
        DepthStencilTarget = depthStencilTarget;
    }

    public new VKContext Context => (VKContext)base.Context;

    public TextureView[] ColorTargets { get; }

    public TextureView? DepthStencilTarget { get; }

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
