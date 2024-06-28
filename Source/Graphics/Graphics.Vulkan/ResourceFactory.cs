namespace Graphics.Vulkan;

public class ResourceFactory : ContextObject
{
    private readonly GraphicsDevice _graphicsDevice;

    internal ResourceFactory(Context context, GraphicsDevice graphicsDevice) : base(context)
    {
        _graphicsDevice = graphicsDevice;
    }

    protected override void Destroy()
    {
    }
}
