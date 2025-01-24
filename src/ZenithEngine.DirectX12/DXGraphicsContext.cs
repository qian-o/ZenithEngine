using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXGraphicsContext : GraphicsContext
{
    public ComPtr<ID3D12Device> Device;

    public DXGraphicsContext()
    {
        D3D12 = D3D12.GetApi();
        Backend = Backend.DirectX12;
        Capabilities = new(this);
        Factory = new(this);
    }

    public D3D12 D3D12 { get; }

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
        if (Device.Handle is not null)
        {
            return;
        }

        if (useDebugLayer)
        {
            D3D12.GetDebugInterface(out ComPtr<ID3D12Debug> debugInterface).ThrowIfError();

            debugInterface.EnableDebugLayer();

            debugInterface.Dispose();
        }

        D3D12.CreateDevice(ref Unsafe.NullRef<IUnknown>(), D3DFeatureLevel.Level120, out Device).ThrowIfError();

        Capabilities.Init();
    }

    protected override void DestroyInternal()
    {
        Device.Dispose();
    }
}
