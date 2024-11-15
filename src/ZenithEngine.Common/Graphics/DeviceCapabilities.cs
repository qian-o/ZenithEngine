namespace ZenithEngine.Common.Graphics;

public abstract class DeviceCapabilities
{
    public abstract bool IsRayQuerySupported { get; }

    public abstract bool IsRayTracingSupported { get; }
}