﻿using Graphics.Core.Helpers;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Engine.Vulkan;

internal unsafe sealed class VKDeviceCapabilities : DeviceCapabilities
{
    public override bool IsRayTracingSupported { get; }

    public override bool IsRayQuerySupported { get; }

    public bool IsDescriptorBufferSupported { get; }

    public void Init(Vk vk, VkPhysicalDevice physicalDevice)
    {
        uint extensionPropertyCount = 0;
        vk.EnumerateDeviceExtensionProperties(physicalDevice, (string)null!, &extensionPropertyCount, null);

        ExtensionProperties[] extensionProperties = new ExtensionProperties[extensionPropertyCount];
        vk.EnumerateDeviceExtensionProperties(physicalDevice, (string)null!, &extensionPropertyCount, extensionProperties);

        this.SetPropertyValue(nameof(IsRayQuerySupported), SupportsExtension(extensionProperties, KhrRayTracingPipeline.ExtensionName));
        this.SetPropertyValue(nameof(IsRayTracingSupported), SupportsExtension(extensionProperties, KhrRayQuery.ExtensionName));
        this.SetPropertyValue(nameof(IsDescriptorBufferSupported), SupportsExtension(extensionProperties, ExtDescriptorBuffer.ExtensionName));
    }

    private static bool SupportsExtension(ExtensionProperties[] extensionProperties, string extensionName)
    {
        foreach (ExtensionProperties extensionProperty in extensionProperties)
        {
            if (Alloter.Get(extensionProperty.ExtensionName) == extensionName)
            {
                return true;
            }
        }

        return false;
    }
}
