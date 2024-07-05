namespace Graphics.Vulkan;

public unsafe class ResourceSet : DeviceResource
{
    public ResourceSet(GraphicsDevice graphicsDevice, ref readonly ResourceSetDescription description) : base(graphicsDevice)
    {
    }

    protected override void Destroy()
    {
    }
}
