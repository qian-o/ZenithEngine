﻿using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using ZenithEngine.Common;

namespace ZenithEngine.Vulkan;

internal unsafe partial class VKGraphicsContext
{
    public VkDevice Device;

    public VkQueue GraphicsQueue { get; private set; }

    public VkQueue ComputeQueue { get; private set; }

    public VkQueue CopyQueue { get; private set; }

    public KhrSwapchain? KhrSwapchain { get; private set; }

    public KhrRayTracingPipeline? KhrRayTracingPipeline { get; private set; }

    public KhrAccelerationStructure? KhrAccelerationStructure { get; private set; }

    public KhrDeferredHostOperations? KhrDeferredHostOperations { get; private set; }

    public VKDescriptorSetAllocator? DescriptorSetAllocator { get; private set; }

    private void InitDevice()
    {
        using MemoryAllocator allocator = new();

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            PQueueCreateInfos = QueueCreateInfos(allocator, out uint infoCount),
            QueueCreateInfoCount = infoCount,
            PpEnabledExtensionNames = DeviceExtensions(allocator, out uint extensionCount),
            EnabledExtensionCount = extensionCount
        };

        createInfo.AddNext(out PhysicalDeviceFeatures2 features2)
                  .AddNext(out PhysicalDeviceVulkan13Features _)
                  .AddNext(out PhysicalDeviceVulkan12Features _)
                  .AddNext(out PhysicalDeviceVulkan11Features _);

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

        Vk.GetPhysicalDeviceFeatures2(PhysicalDevice, &features2);

        Vk.CreateDevice(PhysicalDevice, &createInfo, null, out Device).ThrowIfError();

        GraphicsQueue = Vk.GetDeviceQueue(Device, GraphicsQueueFamilyIndex, 0);
        ComputeQueue = Vk.GetDeviceQueue(Device, ComputeQueueFamilyIndex, 0);
        CopyQueue = Vk.GetDeviceQueue(Device, CopyQueueFamilyIndex, 0);
        KhrSwapchain = Vk.GetExtension<KhrSwapchain>(Instance, Device);
        KhrRayTracingPipeline = Vk.GetExtension<KhrRayTracingPipeline>(Instance, Device);
        KhrAccelerationStructure = Vk.GetExtension<KhrAccelerationStructure>(Instance, Device);
        KhrDeferredHostOperations = Vk.GetExtension<KhrDeferredHostOperations>(Instance, Device);
        DescriptorSetAllocator = new(this);
    }

    private void DestroyDevice()
    {
        DescriptorSetAllocator?.Dispose();
        KhrDeferredHostOperations?.Dispose();
        KhrAccelerationStructure?.Dispose();
        KhrRayTracingPipeline?.Dispose();
        KhrSwapchain?.Dispose();

        Vk.DestroyDevice(Device, null);

        KhrSwapchain = null;
        KhrRayTracingPipeline = null;
        KhrAccelerationStructure = null;
        KhrDeferredHostOperations = null;
        DescriptorSetAllocator = null;
    }

    private DeviceQueueCreateInfo* QueueCreateInfos(MemoryAllocator allocator, out uint count)
    {
        float* queuePriorities = allocator.Alloc([1.0f]);

        DeviceQueueCreateInfo[] queueCreateInfos = [.. QueueFamilyIndices!.Select(Info)];

        count = (uint)queueCreateInfos.Length;

        return allocator.Alloc(queueCreateInfos);

        DeviceQueueCreateInfo Info(uint queueFamilyIndex)
        {
            return new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = queueFamilyIndex,
                QueueCount = 1,
                PQueuePriorities = queuePriorities
            };
        }
    }

    private byte** DeviceExtensions(MemoryAllocator allocator, out uint count)
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
            extensions =
            [
                .. extensions,
                KhrAccelerationStructure.ExtensionName,
                KhrDeferredHostOperations.ExtensionName
            ];
        }

        count = (uint)extensions.Length;

        return allocator.AllocUTF8(extensions);
    }
}
