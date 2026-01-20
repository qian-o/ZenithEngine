using Metal;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal unsafe class MTLGraphicsContext : GraphicsContext
{
    public MTLGraphicsContext()
    {
        IMTLDevice device;
        if (MTLDevice.SystemDefault is null)
        {
            throw new InvalidOperationException("No Metal-compatible device found.");
        }
        else
        {
            device = MTLDevice.SystemDefault;
        }

        if (!device.SupportsFamily(MTLGpuFamily.Metal4))
        {
            throw new NotSupportedException("The selected Metal device does not support the metal 4 feature set required by this engine.");
        }

        Device = device;
        Backend = Backend.Metal;
        Capabilities = new MTLDeviceCapabilities(device);
        Factory = new MTLResourceFactory(this);
    }

    public IMTLDevice Device { get; }

    public override Backend Backend { get; }

    public override DeviceCapabilities Capabilities { get; }

    public override ResourceFactory Factory { get; }

    public override MappedResource MapMemory(Buffer buffer, MapMode mode)
    {
        throw new NotImplementedException();
    }

    public override void UnmapMemory(Buffer buffer)
    {
        throw new NotImplementedException();
    }

    protected override void CreateDeviceInternal(bool useDebugLayer)
    {
        if (useDebugLayer)
        {
            // Note: Metal's debug layer is enabled via environment variables or Xcode settings,
            // so there's no direct API call to enable it like in DirectX or Vulkan.
        }
    }

    protected override void DestroyInternal()
    {
        throw new NotImplementedException();
    }
}
