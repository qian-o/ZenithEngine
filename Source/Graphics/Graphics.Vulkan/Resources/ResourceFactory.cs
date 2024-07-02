namespace Graphics.Vulkan;

public class ResourceFactory : ContextObject
{
    private readonly GraphicsDevice _graphicsDevice;

    internal ResourceFactory(Context context, GraphicsDevice graphicsDevice) : base(context)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Buffer CreateBuffer(in BufferDescription description)
    {
        return new Buffer(_graphicsDevice, description);
    }

    public Texture CreateTexture(in TextureDescription description)
    {
        return new Texture(_graphicsDevice, description);
    }

    public TextureView CreateTextureView(Texture target)
    {
        return CreateTextureView(new TextureViewDescription(target));
    }

    public TextureView CreateTextureView(in TextureViewDescription description)
    {
        return new TextureView(_graphicsDevice, description);
    }

    public Framebuffer CreateFramebuffer(in FramebufferDescription description)
    {
        return new Framebuffer(_graphicsDevice, description);
    }

    public Swapchain CreateSwapchain(in SwapchainDescription description)
    {
        return new Swapchain(_graphicsDevice, description);
    }

    protected override void Destroy()
    {
    }
}
