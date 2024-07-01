namespace Graphics.Vulkan;

public class ResourceFactory : ContextObject
{
    private readonly GraphicsDevice _graphicsDevice;

    internal ResourceFactory(Context context, GraphicsDevice graphicsDevice) : base(context)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Texture CreateTexture(in TextureDescription description)
    {
        return new Texture(_graphicsDevice, description);
    }

    protected override void Destroy()
    {
    }
}
