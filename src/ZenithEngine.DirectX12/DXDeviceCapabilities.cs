using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using ZenithEngine.Common;
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
        AdapterDesc desc;
        context.Adapter.GetDesc(&desc).ThrowIfError();

        deviceName = Utils.PtrToStringUni((nint)desc.Description);

        if (context.Device5.Handle is not null)
        {
            isRayQuerySupported = true;
            isRayTracingSupported = true;
        }
        else
        {
            isRayQuerySupported = false;
            isRayTracingSupported = false;
        }
    }
}
