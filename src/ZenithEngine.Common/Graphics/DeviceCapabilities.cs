namespace ZenithEngine.Common.Graphics;

public abstract class DeviceCapabilities
{
    public abstract string DeviceName { get; }

    public abstract bool IsRayQuerySupported { get; }

    public abstract bool IsRayTracingSupported { get; }
}