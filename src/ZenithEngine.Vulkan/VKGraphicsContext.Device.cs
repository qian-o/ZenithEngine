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

    public VKQueueAllocator DirectQueueAllocator { get; private set; }

    public VKQueueAllocator CopyQueueAllocator { get; private set; }

    public KhrSwapchain? KhrSwapchain { get; private set; }

    public KhrRayTracingPipeline? KhrRayTracingPipeline { get; private set; }

    public KhrAccelerationStructure? KhrAccelerationStructure { get; private set; }

    public KhrDeferredHostOperations? KhrDeferredHostOperations { get; private set; }

    public VKDescriptorSetAllocator? DescriptorSetAllocator { get; private set; }

    public VkQueue GetQueue(CommandProcessorType type)
    {
        return type switch
        {
            CommandProcessorType.Direct => DirectQueueAllocator.Alloc(),
            CommandProcessorType.Copy => CopyQueueAllocator.Alloc(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public void FreeQueue(CommandProcessorType type, VkQueue queue)
    {
        switch (type)
        {
            case CommandProcessorType.Direct:
                DirectQueueAllocator.Free(queue);
                break;
            case CommandProcessorType.Copy:
                CopyQueueAllocator.Free(queue);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

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

        uint directQueueCount = 0;
        uint copyQueueCount = 0;
        for (uint i = 0; i < properties.Length; i++)
        {
            QueueFlags flags = properties[i].QueueFlags;
            uint count = properties[i].QueueCount;

            if (flags.HasFlag(QueueFlags.GraphicsBit
                              | QueueFlags.ComputeBit
                              | QueueFlags.TransferBit) && directQueueCount < count)
            {
                DirectQueueFamilyIndex = i;
                directQueueCount = count;
            }
            else if (flags.HasFlag(QueueFlags.TransferBit) && copyQueueCount < count)
            {
                CopyQueueFamilyIndex = i;
                copyQueueCount = count;
            }
        }

        if (CopyQueueFamilyIndex is 0)
        {
            CopyQueueFamilyIndex = DirectQueueFamilyIndex;
            copyQueueCount = directQueueCount;
        }

        SharingEnabled = DirectQueueFamilyIndex != CopyQueueFamilyIndex;

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo
        };

        float* directQueuePriorities = allocator.Alloc<float>(directQueueCount);
        float* copyQueuePriorities = allocator.Alloc<float>(copyQueueCount);

        for (uint i = 0; i < directQueueCount; i++)
        {
            directQueuePriorities[i] = 1;
        }

        for (uint i = 0; i < copyQueueCount; i++)
        {
            copyQueuePriorities[i] = 1;
        }

        DeviceQueueCreateInfo* queueCreateInfos = allocator.Alloc<DeviceQueueCreateInfo>(2);

        queueCreateInfos[0] = new()
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = DirectQueueFamilyIndex,
            QueueCount = directQueueCount,
            PQueuePriorities = directQueuePriorities
        };

        queueCreateInfos[1] = new()
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = CopyQueueFamilyIndex,
            QueueCount = copyQueueCount,
            PQueuePriorities = copyQueuePriorities
        };

        createInfo.QueueCreateInfoCount = SharingEnabled ? 2u : 1u;
        createInfo.PQueueCreateInfos = queueCreateInfos;

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

        DirectQueueAllocator = new(this, DirectQueueFamilyIndex, directQueueCount);
        CopyQueueAllocator = SharingEnabled ? new(this, CopyQueueFamilyIndex, copyQueueCount) : DirectQueueAllocator;
        KhrSwapchain = Vk.TryGetExtension<KhrSwapchain>(Instance, Device);
        KhrRayTracingPipeline = Vk.TryGetExtension<KhrRayTracingPipeline>(Instance, Device);
        KhrAccelerationStructure = Vk.TryGetExtension<KhrAccelerationStructure>(Instance, Device);
        KhrDeferredHostOperations = Vk.TryGetExtension<KhrDeferredHostOperations>(Instance, Device);
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
}
