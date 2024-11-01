namespace Graphics.Engine;

public abstract class DeviceCapabilities
{
    public abstract bool IsRayTracingSupported { get; }

    public abstract bool IsRayQuerySupported { get; }
}
