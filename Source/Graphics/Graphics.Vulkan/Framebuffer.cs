namespace Graphics.Vulkan;

public class Framebuffer : Resource
{
    public Framebuffer(GraphicsDevice graphicsDevice) : base(graphicsDevice)
    {
    }

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
