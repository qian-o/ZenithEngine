namespace Graphics.Vulkan;

public class Framebuffer : DeviceResource
{
    public Framebuffer(GraphicsDevice graphicsDevice, ref readonly FramebufferDescription description, bool isPresented) : base(graphicsDevice)
    {
    }

    protected override void Destroy()
    {
    }
}
