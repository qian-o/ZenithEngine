namespace Graphics.Vulkan;

public unsafe class Pipeline : DeviceResource
{
    internal Pipeline(GraphicsDevice graphicsDevice, ref readonly GraphicsPipelineDescription description) : base(graphicsDevice)
    {
    }

    protected override void Destroy()
    {
    }
}
