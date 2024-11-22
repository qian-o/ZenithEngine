using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDeviceCapabilities : DeviceCapabilities
{
    private bool isRayQuerySupported;
    private bool isRayTracingSupported;

    public override bool IsRayQuerySupported => isRayQuerySupported;

    public override bool IsRayTracingSupported => isRayTracingSupported;

    public void Init(VKGraphicsContext context)
    {
        uint extensionPropertyCount = 0;
        context.Vk.EnumerateDeviceExtensionProperties(context.PhysicalDevice, (string)null!, &extensionPropertyCount, null);

        ExtensionProperties[] extensionProperties = new ExtensionProperties[extensionPropertyCount];
        context.Vk.EnumerateDeviceExtensionProperties(context.PhysicalDevice, (string)null!, &extensionPropertyCount, extensionProperties);

        isRayQuerySupported = SupportsExtension(extensionProperties, KhrRayQuery.ExtensionName);
        isRayTracingSupported = SupportsExtension(extensionProperties, KhrRayTracingPipeline.ExtensionName);
    }

    private static bool SupportsExtension(ExtensionProperties[] extensionProperties, string extensionName)
    {
        foreach (ExtensionProperties extensionProperty in extensionProperties)
        {
            if (extensionName == Utils.PtrToStringAnsi((nint)extensionProperty.ExtensionName))
            {
                return true;
            }
        }

        return false;
    }
}
