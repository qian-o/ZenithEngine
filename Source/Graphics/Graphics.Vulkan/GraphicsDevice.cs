using Graphics.Core;

namespace Graphics.Vulkan;

public class GraphicsDevice : DisposableObject
{
    protected override void Destroy()
    {
    }
}

public unsafe partial class Context
{
    public GraphicsDevice CreateGraphicsDevice(PhysicalDevice physicalDevice, Window window)
    {
        throw new NotImplementedException();
    }
}
