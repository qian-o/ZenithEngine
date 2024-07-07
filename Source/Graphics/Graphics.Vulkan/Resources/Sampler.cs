namespace Graphics.Vulkan;

public unsafe class Sampler : DeviceResource
{
    private readonly VkSampler _sampler;

    internal Sampler(GraphicsDevice graphicsDevice, ref readonly SamplerDescription description) : base(graphicsDevice)
    {
    }

    internal VkSampler Handle => _sampler;

    protected override void Destroy()
    {
        Vk.DestroySampler(Device, _sampler, null);
    }
}
