using Graphics.Core.Helpers;
using Graphics.Engine.Exceptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Engine.Vulkan;

internal unsafe partial class VKContext
{
    public VkDevice Device { get; private set; }

    public uint GraphicsFamilyIndex { get; private set; }

    public uint ComputeFamilyIndex { get; private set; }

    public uint TransferFamilyIndex { get; private set; }

    public VkQueue GraphicsQueue { get; private set; }

    public VkQueue ComputeQueue { get; private set; }

    public VkQueue TransferQueue { get; private set; }

    public KhrSwapchain KhrSwapchain { get; private set; } = null!;

    public KhrRayTracingPipeline? KhrRayTracingPipeline { get; private set; }

    public KhrAccelerationStructure? KhrAccelerationStructure { get; private set; }

    public KhrDeferredHostOperations? KhrDeferredHostOperations { get; private set; }

    private void InitDevice()
    {
        using Allocator allocator = new();

        string[] extensions = GetDeviceExtensions();

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = allocator.Alloc(extensions)
        };

        // Init queues
        {
            float queuePriorities = 1.0f;

            GraphicsFamilyIndex = GetGraphicsQueueFamilyIndex(QueueFlags.GraphicsBit);
            ComputeFamilyIndex = GetGraphicsQueueFamilyIndex(QueueFlags.ComputeBit);
            TransferFamilyIndex = GetGraphicsQueueFamilyIndex(QueueFlags.TransferBit);

            HashSet<uint> queueFamilyIndices = [GraphicsFamilyIndex, ComputeFamilyIndex, TransferFamilyIndex];

            DeviceQueueCreateInfo[] deviceQueueCreateInfos = new DeviceQueueCreateInfo[queueFamilyIndices.Count];

            for (int i = 0; i < queueFamilyIndices.Count; i++)
            {
                DeviceQueueCreateInfo deviceQueueCreateInfo = new()
                {
                    SType = StructureType.DeviceQueueCreateInfo,
                    QueueFamilyIndex = queueFamilyIndices.ElementAt(i),
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
        }

        VkDevice device;
        Vk.CreateDevice(PhysicalDevice, &createInfo, null, &device).ThrowCode();

        VkQueue graphicsQueue;
        Vk.GetDeviceQueue(device, GraphicsFamilyIndex, 0, &graphicsQueue);

        VkQueue computeQueue;
        Vk.GetDeviceQueue(device, ComputeFamilyIndex, 0, &computeQueue);

        VkQueue transferQueue;
        Vk.GetDeviceQueue(device, TransferFamilyIndex, 0, &transferQueue);

        Device = device;
        GraphicsQueue = graphicsQueue;
        ComputeQueue = computeQueue;
        TransferQueue = transferQueue;
        KhrSwapchain = Vk.GetExtension<KhrSwapchain>(Instance, Device);
        KhrRayTracingPipeline = extensions.Contains(KhrRayTracingPipeline.ExtensionName) ? Vk.GetExtension<KhrRayTracingPipeline>(Instance, Device) : null;
        KhrAccelerationStructure = extensions.Contains(KhrAccelerationStructure.ExtensionName) ? Vk.GetExtension<KhrAccelerationStructure>(Instance, Device) : null;
        KhrDeferredHostOperations = extensions.Contains(KhrDeferredHostOperations.ExtensionName) ? Vk.GetExtension<KhrDeferredHostOperations>(Instance, Device) : null;
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
