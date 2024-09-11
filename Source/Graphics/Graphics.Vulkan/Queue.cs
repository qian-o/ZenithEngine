namespace Graphics.Vulkan;

internal sealed unsafe class Queue : DeviceResource
{
    private readonly VkQueue _queue;
    private readonly uint _queueFamilyIndex;

    public Queue(GraphicsDevice graphicsDevice, uint queueFamilyIndex) : base(graphicsDevice)
    {
        VkQueue queue;
        Vk.GetDeviceQueue(Device, queueFamilyIndex, 0, &queue);

        _queue = queue;
        _queueFamilyIndex = queueFamilyIndex;
    }

    public VkQueue Handle => _queue;

    public uint FamilyIndex => _queueFamilyIndex;

    protected override void Destroy()
    {
    }
}
