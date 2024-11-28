using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Vulkan;

internal unsafe partial class VKGraphicsContext
{
    public VkDevice Device;

    public uint DirectQueueFamilyIndex { get; private set; }

    public uint CopyQueueFamilyIndex { get; private set; }

    public bool SharingEnabled { get; private set; }

    public KhrSwapchain? KhrSwapchain { get; private set; }

    public KhrRayTracingPipeline? KhrRayTracingPipeline { get; private set; }

    public KhrAccelerationStructure? KhrAccelerationStructure { get; private set; }

    public KhrDeferredHostOperations? KhrDeferredHostOperations { get; private set; }

    public uint FindQueueFamilyIndex(CommandProcessorType type)
    {
        return type switch
        {
            CommandProcessorType.Direct => DirectQueueFamilyIndex,
            CommandProcessorType.Copy => CopyQueueFamilyIndex,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private void InitDevice()
    {
        using MemoryAllocator allocator = new();

        uint propertyCount = 0;
        Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &propertyCount, null);

        QueueFamilyProperties[] properties = new QueueFamilyProperties[propertyCount];
        Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &propertyCount, properties);

        for (uint i = 0; i < properties.Length; i++)
        {
            QueueFlags flags = properties[i].QueueFlags;

            if (flags.HasFlag(QueueFlags.GraphicsBit | QueueFlags.ComputeBit | QueueFlags.TransferBit))
            {
                DirectQueueFamilyIndex = i;
            }

            if (flags.HasFlag(QueueFlags.TransferBit))
            {
                CopyQueueFamilyIndex = i;
            }
        }

        SharingEnabled = DirectQueueFamilyIndex != CopyQueueFamilyIndex;

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo
        };

        float queuePriority = 1;

        if (SharingEnabled)
        {
            DeviceQueueCreateInfo* queueCreateInfos = allocator.Alloc<DeviceQueueCreateInfo>(2);

            queueCreateInfos[0] = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = DirectQueueFamilyIndex,
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };

            queueCreateInfos[1] = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = CopyQueueFamilyIndex,
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };

            createInfo.QueueCreateInfoCount = 2;
            createInfo.PQueueCreateInfos = queueCreateInfos;
        }
        else
        {
            DeviceQueueCreateInfo queueCreateInfo = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = DirectQueueFamilyIndex,
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };

            createInfo.QueueCreateInfoCount = 1;
            createInfo.PQueueCreateInfos = &queueCreateInfo;
        }

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

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = allocator.AllocUTF8(extensions);

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

        Vk.GetPhysicalDeviceFeatures2(PhysicalDevice, &features2);

        Vk.CreateDevice(PhysicalDevice, &createInfo, null, out Device).ThrowIfError();

        KhrSwapchain = Vk.TryGetExtension<KhrSwapchain>(Instance, Device);
        KhrRayTracingPipeline = Vk.TryGetExtension<KhrRayTracingPipeline>(Instance, Device);
        KhrAccelerationStructure = Vk.TryGetExtension<KhrAccelerationStructure>(Instance, Device);
        KhrDeferredHostOperations = Vk.TryGetExtension<KhrDeferredHostOperations>(Instance, Device);
    }

    private void DestroyDevice()
    {
        KhrDeferredHostOperations?.Dispose();
        KhrAccelerationStructure?.Dispose();
        KhrRayTracingPipeline?.Dispose();
        KhrSwapchain?.Dispose();

        Vk.DestroyDevice(Device, null);

        KhrSwapchain = null;
        KhrRayTracingPipeline = null;
        KhrAccelerationStructure = null;
        KhrDeferredHostOperations = null;
    }
}
