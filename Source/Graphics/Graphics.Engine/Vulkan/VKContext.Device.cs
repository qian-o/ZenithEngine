namespace Graphics.Engine.Vulkan;

internal unsafe partial class VKContext
{
    public VkDevice Device { get; private set; }

    private void InitDevice()
    {
        // TODO: Create logical device.
        Device = default;
    }
}
