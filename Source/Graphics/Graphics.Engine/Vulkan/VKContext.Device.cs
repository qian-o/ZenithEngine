using Graphics.Core.Helpers;
using Graphics.Engine.Exceptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Engine.Vulkan;

internal unsafe partial class VKContext
{
    public VkDevice Device { get; private set; }

    public uint GraphicsIndex { get; private set; }

    public uint ComputeIndex { get; private set; }

    public uint TransferIndex { get; private set; }

    private void InitDevice()
    {
        using Alloter alloter = new();

        string[] extensions = GetDeviceExtensions();

        DeviceCreateInfo.Chain(out DeviceCreateInfo createInfo);
        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = alloter.Alloc(extensions);

        // Init queues
        {
            float queuePriorities = 1.0f;

            uint queueFamilyPropertyCount = 0;
            Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyPropertyCount, null);

            QueueFamilyProperties[] queueFamilyProperties = new QueueFamilyProperties[queueFamilyPropertyCount];
            Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyPropertyCount, queueFamilyProperties.AsPointer());

            GraphicsIndex = GetGraphicsQueueFamilyIndex(queueFamilyProperties, QueueFlags.GraphicsBit);
            ComputeIndex = GetGraphicsQueueFamilyIndex(queueFamilyProperties, QueueFlags.ComputeBit);
            TransferIndex = GetGraphicsQueueFamilyIndex(queueFamilyProperties, QueueFlags.TransferBit);

            HashSet<uint> queueFamilies = [GraphicsIndex, ComputeIndex, TransferIndex];

            DeviceQueueCreateInfo[] deviceQueueCreateInfos = new DeviceQueueCreateInfo[queueFamilies.Count];

            for (int i = 0; i < queueFamilies.Count; i++)
            {
                DeviceQueueCreateInfo.Chain(out DeviceQueueCreateInfo deviceQueueCreateInfo);
                deviceQueueCreateInfo.QueueFamilyIndex = queueFamilies.ElementAt(i);
                deviceQueueCreateInfo.QueueCount = 1;
                deviceQueueCreateInfo.PQueuePriorities = &queuePriorities;

                deviceQueueCreateInfos[i] = deviceQueueCreateInfo;
            }

            createInfo.QueueCreateInfoCount = (uint)deviceQueueCreateInfos.Length;
            createInfo.PQueueCreateInfos = deviceQueueCreateInfos.AsPointer();
        }

        // Init features
        {
            createInfo.AddNext(out PhysicalDeviceFeatures2 features2)
                      .AddNext(out PhysicalDeviceVulkan13Features _)
                      .AddNext(out PhysicalDeviceScalarBlockLayoutFeatures _)
                      .AddNext(out PhysicalDeviceDescriptorIndexingFeatures _)
                      .AddNext(out PhysicalDeviceBufferDeviceAddressFeatures _);

            if (Capabilities.IsRayQuerySupported)
            {
                createInfo.AddNext(out PhysicalDeviceRayQueryFeaturesKHR _);
            }

            if (Capabilities.IsRayTracingSupported)
            {
                createInfo.AddNext(out PhysicalDeviceRayTracingPipelineFeaturesKHR _);
            }

            if (Capabilities.IsDescriptorBufferSupported)
            {
                createInfo.AddNext(out PhysicalDeviceDescriptorBufferFeaturesEXT _);
            }

            if (Capabilities.IsRayQuerySupported || Capabilities.IsRayTracingSupported)
            {
                createInfo.AddNext(out PhysicalDeviceAccelerationStructureFeaturesKHR _);
            }

            Vk.GetPhysicalDeviceFeatures2(PhysicalDevice, &features2);
        }

        VkDevice device;
        Vk.CreateDevice(PhysicalDevice, &createInfo, null, &device).ThrowCode("Failed to create logical device");

        Device = device;
    }

    private static uint GetGraphicsQueueFamilyIndex(QueueFamilyProperties[] queueFamilyProperties, QueueFlags flags)
    {
        for (uint i = 0; i < queueFamilyProperties.Length; i++)
        {
            if (queueFamilyProperties[i].QueueFlags.HasFlag(flags))
            {
                return i;
            }
        }

        throw new BackendException("No graphics queue family found.");
    }

    private string[] GetDeviceExtensions()
    {
        string[] extensions = [KhrSwapchain.ExtensionName];

        if (Capabilities.IsRayQuerySupported)
        {
            extensions = [.. extensions, KhrRayQuery.ExtensionName];
        }

        if (Capabilities.IsRayTracingSupported)
        {
            extensions = [.. extensions, KhrRayTracingPipeline.ExtensionName];
        }

        if (Capabilities.IsDescriptorBufferSupported)
        {
            extensions = [.. extensions, ExtDescriptorBuffer.ExtensionName];
        }

        if (Capabilities.IsRayQuerySupported || Capabilities.IsRayTracingSupported)
        {
            extensions = [.. extensions, KhrAccelerationStructure.ExtensionName, KhrDeferredHostOperations.ExtensionName];
        }

        return extensions;
    }
}
