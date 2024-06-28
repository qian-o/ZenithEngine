namespace Graphics.Vulkan;

public class SwapChain : ContextObject
{
    private readonly GraphicsDevice _graphicsDevice;

    internal SwapChain(Context context, GraphicsDevice graphicsDevice) : base(context)
    {
        _graphicsDevice = graphicsDevice;
    }

    internal GraphicsDevice GraphicsDevice => _graphicsDevice;

    protected override void Destroy()
    {
    }
}
