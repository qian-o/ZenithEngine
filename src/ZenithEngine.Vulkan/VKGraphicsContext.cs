using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal class VKGraphicsContext : GraphicsContext
{
    public VKGraphicsContext()
    {
        Backend = Backend.Vulkan;
        Capabilities = new VKDeviceCapabilities();
        Factory = new VKResourceFactory(this);
    }

    public override Backend Backend { get; }

    public override DeviceCapabilities Capabilities { get; }

    public override ResourceFactory Factory { get; }

    public override void CreateDeviceInternal(bool useDebugLayer)
    {
        throw new NotImplementedException();
    }

    public override MappedResource MapMemory(Buffer buffer, MapMode mode)
    {
        throw new NotImplementedException();
    }

    public override void UnmapMemory(Buffer buffer)
    {
        throw new NotImplementedException();
    }

    protected override void DestroyInternal()
    {
        throw new NotImplementedException();
    }
}
