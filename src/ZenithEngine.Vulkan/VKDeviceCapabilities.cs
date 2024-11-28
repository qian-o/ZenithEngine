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
        uint propertyCount = 0;
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
