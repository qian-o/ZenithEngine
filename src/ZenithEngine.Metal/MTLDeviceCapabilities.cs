using Metal;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLDeviceCapabilities(IMTLDevice device) : DeviceCapabilities
{
    public override string DeviceName { get; } = device.Name;

    public override bool IsRayTracingSupported { get; } = device.SupportsRaytracing || device.SupportsRaytracingFromRender;

    public override bool IsMeshShaderSupported { get; } = device.SupportsFamily(MTLGpuFamily.Mac2) || device.SupportsFamily(MTLGpuFamily.Apple7);
}
