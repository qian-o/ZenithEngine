using System.Runtime.InteropServices;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common.Graphics;
using Feature = Silk.NET.Direct3D12.Feature;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDeviceCapabilities(DXGraphicsContext context) : DeviceCapabilities
{
    private string deviceName = "Unknown";
    private bool isRayTracingSupported;
    private bool isMeshShaderSupported;

    public override string DeviceName => deviceName;

    public override bool IsRayTracingSupported => isRayTracingSupported;

    public override bool IsMeshShaderSupported => isMeshShaderSupported;

    public void Init()
    {
        AdapterDesc desc;
        context.Adapter.GetDesc(&desc).ThrowIfError();

        deviceName = Marshal.PtrToStringUni((nint)desc.Description)!;

        FeatureDataD3D12Options5 options5 = new();
        FeatureDataD3D12Options7 options7 = new();

        context.Device.CheckFeatureSupport(Feature.D3D12Options5, &options5, (uint)sizeof(FeatureDataD3D12Options5)).ThrowIfError();
        context.Device.CheckFeatureSupport(Feature.D3D12Options7, &options7, (uint)sizeof(FeatureDataD3D12Options7)).ThrowIfError();

        isRayTracingSupported = options5.RaytracingTier is not RaytracingTier.TierNotSupported;
        isMeshShaderSupported = options7.MeshShaderTier is not MeshShaderTier.TierNotSupported;
    }
}
