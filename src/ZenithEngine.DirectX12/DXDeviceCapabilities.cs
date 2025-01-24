using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDeviceCapabilities(DXGraphicsContext context) : DeviceCapabilities
{
    private string deviceName = "Unknown";
    private bool isRayQuerySupported;
    private bool isRayTracingSupported;

    public override string DeviceName => deviceName;

    public override bool IsRayQuerySupported => isRayQuerySupported;

    public override bool IsRayTracingSupported => isRayTracingSupported;

    public void Init()
    {
        deviceName = "DirectX 12";

        using ComPtr<ID3D12Device5> device5 = context.Device.QueryInterface<ID3D12Device5>();

        if (device5.Handle is not null)
        {
            isRayQuerySupported = true;
            isRayTracingSupported = true;

            device5.Dispose();
        }
    }
}
