namespace Graphics.Vulkan;

public unsafe class Framebuffer : DeviceResource
{
    private readonly TextureView _colorAttachment;
    private readonly TextureView? _depthAttachment;

    public Framebuffer(GraphicsDevice graphicsDevice, in FramebufferDescription description) : base(graphicsDevice)
    {
        TextureViewDescription colorDescription = new(description.ColorTarget.Target)
        {
            BaseMipLevel = description.ColorTarget.MipLevel,
            BaseArrayLayer = description.ColorTarget.ArrayLayer
        };

        _colorAttachment = new TextureView(graphicsDevice, colorDescription);

        if (description.DepthTarget != null)
        {
            TextureViewDescription depthDescription = new(description.DepthTarget.Value.Target)
            {
                BaseMipLevel = description.DepthTarget.Value.MipLevel,
                BaseArrayLayer = description.DepthTarget.Value.ArrayLayer
            };

            _depthAttachment = new TextureView(graphicsDevice, depthDescription);
        }
    }

    internal TextureView ColorAttachment => _colorAttachment;

    internal TextureView? DepthAttachment => _depthAttachment;

    protected override void Destroy()
    {
        _colorAttachment.Dispose();
        _depthAttachment?.Dispose();
    }
}
