namespace Graphics.Vulkan;

internal sealed unsafe class Queue : DeviceResource
{
    public Queue(GraphicsDevice graphicsDevice, uint queueFamilyIndex) : base(graphicsDevice)
    {
        VkQueue queue;
        Vk.GetDeviceQueue(Device, queueFamilyIndex, 0, &queue);

        Handle = queue;
        FamilyIndex = queueFamilyIndex;
    }

    public VkQueue Handle { get; }

    public uint FamilyIndex { get; }

    protected override void Destroy()
    {
    }
}
