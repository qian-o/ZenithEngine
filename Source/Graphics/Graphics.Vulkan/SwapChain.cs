using Graphics.Core;

namespace Graphics.Vulkan;

public class SwapChain : DisposableObject
{
    private readonly Context _context;
    private readonly GraphicsDevice _graphicsDevice;

    internal SwapChain(Context context, GraphicsDevice graphicsDevice)
    {
        _context = context;
        _graphicsDevice = graphicsDevice;
    }

    protected override void Destroy()
    {
    }
}
