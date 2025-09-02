using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDeviceCapabilities(VKGraphicsContext context) : DeviceCapabilities
{
    private string deviceName = "Unknown";
    private bool isRayTracingSupported;
    private bool isMeshShaderSupported;

    public override string DeviceName => deviceName;

    public override bool IsRayTracingSupported => isRayTracingSupported;

    public override bool IsMeshShaderSupported => isMeshShaderSupported;

    public void Init()
    {
        PhysicalDeviceProperties deviceProperties;
        context.Vk.GetPhysicalDeviceProperties(context.PhysicalDevice, &deviceProperties);

        deviceName = Marshal.PtrToStringUTF8((nint)deviceProperties.DeviceName)!;

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

        isRayTracingSupported = SupportsExtension(properties, KhrRayTracingPipeline.ExtensionName);
        isMeshShaderSupported = SupportsExtension(properties, ExtMeshShader.ExtensionName);
    }

    private static bool SupportsExtension(ExtensionProperties[] extensionProperties,
                                          string extensionName)
    {
        foreach (ExtensionProperties extensionProperty in extensionProperties)
        {
            if (extensionName == Marshal.PtrToStringUTF8((nint)extensionProperty.ExtensionName))
            {
                return true;
            }
        }

        return false;
    }
}
