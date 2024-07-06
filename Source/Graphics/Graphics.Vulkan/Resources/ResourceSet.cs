namespace Graphics.Vulkan;

public unsafe class ResourceSet : DeviceResource
{
    internal ResourceSet(GraphicsDevice graphicsDevice, ref readonly ResourceSetDescription description) : base(graphicsDevice)
    {
    }

    protected override void Destroy()
    {
    }
}
