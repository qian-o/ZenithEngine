using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXGraphicsContext : GraphicsContext
{
    public ComPtr<IDXGIFactory6> Factory6;
    public ComPtr<IDXGIAdapter> Adapter;
    public ComPtr<ID3D12Device> Device;

    public DXGraphicsContext()
    {
        D3D12 = D3D12.GetApi();
        DXGI = DXGI.GetApi(null);
        Backend = Backend.DirectX12;
        Capabilities = new(this);
        Factory = new(this);
    }

    public D3D12 D3D12 { get; }

    public DXGI DXGI { get; }

    public DXDebug? Debug { get; private set; }

    public override Backend Backend { get; }

    public override DXDeviceCapabilities Capabilities { get; }

    public override DXResourceFactory Factory { get; }

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
        if (Factory6.Handle is not null)
        {
            return;
        }

        if (useDebugLayer)
        {
            D3D12.GetDebugInterface(out ComPtr<ID3D12Debug> debugInterface).ThrowIfError();

            debugInterface.EnableDebugLayer();

            debugInterface.Dispose();
        }

        DXGI.CreateDXGIFactory1(out Factory6).ThrowIfError();

        Factory6.EnumAdapterByGpuPreference(0, GpuPreference.HighPerformance, out Adapter).ThrowIfError();

        D3D12.CreateDevice(Adapter, D3DFeatureLevel.Level120, out Device).ThrowIfError();

        Debug = useDebugLayer ? new(this) : null;

        Capabilities.Init();
    }

    protected override void DestroyInternal()
    {
        Debug?.Dispose();

        Device.Dispose();
        Adapter.Dispose();
        Factory6.Dispose();

        DXGI.Dispose();
        D3D12.Dispose();

        Debug = null;
    }
}
