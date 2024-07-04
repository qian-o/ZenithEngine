namespace Graphics.Vulkan;

public class ResourceFactory : ContextObject
{
    private readonly GraphicsDevice _graphicsDevice;

    internal ResourceFactory(Context context, GraphicsDevice graphicsDevice) : base(context)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Buffer CreateBuffer(ref readonly BufferDescription description)
    {
        return new Buffer(_graphicsDevice, in description);
    }

    public Texture CreateTexture(ref readonly TextureDescription description)
    {
        return new Texture(_graphicsDevice, in description);
    }

    public TextureView CreateTextureView(Texture target)
    {
        TextureViewDescription description = new(target);

        return CreateTextureView(in description);
    }

    public TextureView CreateTextureView(ref readonly TextureViewDescription description)
    {
        return new TextureView(_graphicsDevice, in description);
    }

    public Framebuffer CreateFramebuffer(ref readonly FramebufferDescription description)
    {
        return new Framebuffer(_graphicsDevice, in description);
    }

    public Swapchain CreateSwapchain(ref readonly SwapchainDescription description)
    {
        return new Swapchain(_graphicsDevice, in description);
    }

    protected override void Destroy()
    {
    }
}
