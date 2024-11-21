using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal class VKDeviceCapabilities : DeviceCapabilities
{
    public override bool IsRayQuerySupported { get; }

    public override bool IsRayTracingSupported { get; }
}
