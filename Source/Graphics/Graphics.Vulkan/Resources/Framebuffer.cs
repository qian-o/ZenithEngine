namespace Graphics.Vulkan;

public class Framebuffer : DeviceResource
{
    private readonly TextureView _colorAttachment;
    private readonly TextureView? _resolveColorAttachment;
    private readonly TextureView? _depthAttachment;

    public Framebuffer(GraphicsDevice graphicsDevice, ref readonly FramebufferDescription description) : base(graphicsDevice)
    {
        TextureViewDescription colorDescription = new(description.ColorTarget.Target)
        {
            BaseMipLevel = description.ColorTarget.MipLevel,
            BaseArrayLayer = description.ColorTarget.ArrayLayer
        };

        _colorAttachment = new TextureView(graphicsDevice, in colorDescription);

        if (description.ResolveColorTarget != null)
        {
            TextureViewDescription resolveColorDescription = new(description.ResolveColorTarget.Value.Target)
            {
                BaseMipLevel = description.ResolveColorTarget.Value.MipLevel,
                BaseArrayLayer = description.ResolveColorTarget.Value.ArrayLayer
            };

            _resolveColorAttachment = new TextureView(graphicsDevice, in resolveColorDescription);
        }

        if (description.DepthTarget != null)
        {
            TextureViewDescription depthDescription = new(description.DepthTarget.Value.Target)
            {
                BaseMipLevel = description.DepthTarget.Value.MipLevel,
                BaseArrayLayer = description.DepthTarget.Value.ArrayLayer
            };

            _depthAttachment = new TextureView(graphicsDevice, in depthDescription);
        }
    }

    internal TextureView ColorAttachment => _colorAttachment;

    internal TextureView? ResolveColorAttachment => _resolveColorAttachment;

    internal TextureView? DepthAttachment => _depthAttachment;

    protected override void Destroy()
    {
        _colorAttachment.Dispose();
        _resolveColorAttachment?.Dispose();
        _depthAttachment?.Dispose();
    }
}
