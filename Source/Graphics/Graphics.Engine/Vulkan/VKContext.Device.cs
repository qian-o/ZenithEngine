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

    public KhrSwapchain KhrSwapchain { get; private set; } = null!;

    public KhrRayTracingPipeline? KhrRayTracingPipeline { get; private set; }

    public KhrAccelerationStructure? KhrAccelerationStructure { get; private set; }

    public KhrDeferredHostOperations? KhrDeferredHostOperations { get; private set; }

    public ExtDescriptorBuffer? ExtDescriptorBuffer { get; private set; }

    private void InitDevice()
    {
        using Alloter alloter = new();

        string[] extensions = GetDeviceExtensions();

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = alloter.Alloc(extensions)
        };

        // Init queues
        {
            float queuePriorities = 1.0f;

            GraphicsIndex = GetGraphicsQueueFamilyIndex(QueueFlags.GraphicsBit);
            ComputeIndex = GetGraphicsQueueFamilyIndex(QueueFlags.ComputeBit);
            TransferIndex = GetGraphicsQueueFamilyIndex(QueueFlags.TransferBit);

            HashSet<uint> queueFamilies = [GraphicsIndex, ComputeIndex, TransferIndex];

            DeviceQueueCreateInfo[] deviceQueueCreateInfos = new DeviceQueueCreateInfo[queueFamilies.Count];

            for (int i = 0; i < queueFamilies.Count; i++)
            {
                DeviceQueueCreateInfo deviceQueueCreateInfo = new()
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = queueFamilies.ElementAt(i),
                    QueueCount = 1,
                    PQueuePriorities = &queuePriorities
                };

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

            if (Capabilities.IsRayQuerySupported || Capabilities.IsRayTracingSupported)
            {
                createInfo.AddNext(out PhysicalDeviceAccelerationStructureFeaturesKHR _);
            }

            if (Capabilities.IsDescriptorBufferSupported)
            {
                createInfo.AddNext(out PhysicalDeviceDescriptorBufferFeaturesEXT _);
            }

            Vk.GetPhysicalDeviceFeatures2(PhysicalDevice, &features2);
        }

        VkDevice device;
        Vk.CreateDevice(PhysicalDevice, &createInfo, null, &device).ThrowCode();

        Device = device;
        KhrSwapchain = Vk.GetExtension<KhrSwapchain>(Instance, Device);
        KhrRayTracingPipeline = extensions.Contains(KhrRayTracingPipeline.ExtensionName) ? Vk.GetExtension<KhrRayTracingPipeline>(Instance, Device) : null;
        KhrAccelerationStructure = extensions.Contains(KhrAccelerationStructure.ExtensionName) ? Vk.GetExtension<KhrAccelerationStructure>(Instance, Device) : null;
        KhrDeferredHostOperations = extensions.Contains(KhrDeferredHostOperations.ExtensionName) ? Vk.GetExtension<KhrDeferredHostOperations>(Instance, Device) : null;
        ExtDescriptorBuffer = extensions.Contains(ExtDescriptorBuffer.ExtensionName) ? Vk.GetExtension<ExtDescriptorBuffer>(Instance, Device) : null;
    }

    private void DestroyDevice()
    {
        Vk.DestroyDevice(Device, null);
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

        if (Capabilities.IsRayQuerySupported || Capabilities.IsRayTracingSupported)
        {
            extensions = [.. extensions, KhrAccelerationStructure.ExtensionName, KhrDeferredHostOperations.ExtensionName];
        }

        if (Capabilities.IsDescriptorBufferSupported)
        {
            extensions = [.. extensions, ExtDescriptorBuffer.ExtensionName];
        }

        return extensions;
    }

    private uint GetGraphicsQueueFamilyIndex(QueueFlags flags)
    {
        uint queueFamilyPropertyCount = 0;
        Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyPropertyCount, null);

        QueueFamilyProperties[] queueFamilyProperties = new QueueFamilyProperties[queueFamilyPropertyCount];
        Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queueFamilyPropertyCount, queueFamilyProperties.AsPointer());

        for (uint i = 0; i < queueFamilyProperties.Length; i++)
        {
            if (queueFamilyProperties[i].QueueFlags.HasFlag(flags))
            {
                return i;
            }
        }

        throw new BackendException("No graphics queue family found.");
    }
}
