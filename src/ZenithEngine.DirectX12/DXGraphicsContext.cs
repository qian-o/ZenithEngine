using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXGraphicsContext : GraphicsContext
{
    public DXGraphicsContext()
    {
        DXGI = DXGI.GetApi(null);
        D3D12 = D3D12.GetApi();
        Backend = Backend.DirectX12;
        Capabilities = null!;
        Factory = null!;
    }

    public DXGI DXGI { get; }

    public D3D12 D3D12 { get; }

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
        throw new NotImplementedException();
    }

    protected override void DestroyInternal()
    {
        throw new NotImplementedException();
    }
}
