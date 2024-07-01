namespace Graphics.Vulkan;

public class Texture : DeviceResource
{
    internal Texture(GraphicsDevice graphicsDevice, in TextureDescription description) : base(graphicsDevice)
    {
    }

    protected override void Destroy()
    {
    }
}
