using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDeviceCapabilities(VKGraphicsContext context) : DeviceCapabilities
{
    private string deviceName = "Unknown";
    private bool isRayQuerySupported;
    private bool isRayTracingSupported;

    public override string DeviceName => deviceName;

    public override bool IsRayQuerySupported => isRayQuerySupported;

    public override bool IsRayTracingSupported => isRayTracingSupported;

    public void Init()
    {
        PhysicalDeviceProperties deviceProperties;
        context.Vk.GetPhysicalDeviceProperties(context.PhysicalDevice, &deviceProperties);

        deviceName = Utils.PtrToStringUTF8((nint)deviceProperties.DeviceName);

        uint propertyCount;
        context.Vk.EnumerateDeviceExtensionProperties(context.PhysicalDevice,
                                                      (string)null!,
                                                      &propertyCount,
                                                      null).ThrowIfError();

        ExtensionProperties[] properties = new ExtensionProperties[propertyCount];
        context.Vk.EnumerateDeviceExtensionProperties(context.PhysicalDevice,
                                                      (string)null!,
                                                      &propertyCount,
                                                      properties).ThrowIfError();

        isRayQuerySupported = SupportsExtension(properties, KhrRayQuery.ExtensionName);
        isRayTracingSupported = SupportsExtension(properties, KhrRayTracingPipeline.ExtensionName);
    }

    private static bool SupportsExtension(ExtensionProperties[] extensionProperties,
                                          string extensionName)
    {
        foreach (ExtensionProperties extensionProperty in extensionProperties)
        {
            if (extensionName == Utils.PtrToStringUTF8((nint)extensionProperty.ExtensionName))
            {
                return true;
            }
        }

        return false;
    }
}
