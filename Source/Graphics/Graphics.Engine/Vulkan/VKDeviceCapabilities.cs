namespace Graphics.Engine.Vulkan;

internal class VKDeviceCapabilities : DeviceCapabilities
{
    public override bool IsRayTracingSupported { get; }

    public override bool IsRayQuerySupported { get; }
}
