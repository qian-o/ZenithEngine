namespace Graphics.Vulkan;

public class ResourceFactory : ContextObject
{
    private readonly GraphicsDevice _graphicsDevice;

    internal ResourceFactory(Context context, GraphicsDevice graphicsDevice) : base(context)
    {
        _graphicsDevice = graphicsDevice;
    }

    public DeviceBuffer CreateBuffer(ref readonly BufferDescription description)
    {
        return new DeviceBuffer(_graphicsDevice, in description);
    }

    public DeviceBuffer CreateBuffer(BufferDescription description) => CreateBuffer(in description);

    public Texture CreateTexture(ref readonly TextureDescription description)
    {
        return new Texture(_graphicsDevice, in description);
    }

    public Texture CreateTexture(TextureDescription description) => CreateTexture(in description);

    public TextureView CreateTextureView(Texture target)
    {
        TextureViewDescription description = new(target);

        return CreateTextureView(in description);
    }

    public TextureView CreateTextureView(ref readonly TextureViewDescription description)
    {
        return new TextureView(_graphicsDevice, in description);
    }

    public TextureView CreateTextureView(TextureViewDescription description) => CreateTextureView(in description);

    public Framebuffer CreateFramebuffer(ref readonly FramebufferDescription description)
    {
        return new Framebuffer(_graphicsDevice, in description, false);
    }

    public Framebuffer CreateFramebuffer(FramebufferDescription description) => CreateFramebuffer(in description);

    public Swapchain CreateSwapchain(ref readonly SwapchainDescription description)
    {
        return new Swapchain(_graphicsDevice, in description);
    }

    public Swapchain CreateSwapchain(SwapchainDescription description) => CreateSwapchain(in description);

    public ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDescription description)
    {
        return new ResourceLayout(_graphicsDevice, in description);
    }

    public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description) => CreateResourceLayout(in description);

    public ResourceSet CreateResourceSet(ref readonly ResourceSetDescription description)
    {
        return new ResourceSet(_graphicsDevice, in description);
    }

    public ResourceSet CreateResourceSet(ResourceSetDescription description) => CreateResourceSet(in description);

    protected override void Destroy()
    {
    }
}
