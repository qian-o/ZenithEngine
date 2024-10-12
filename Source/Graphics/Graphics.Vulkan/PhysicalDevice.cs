using Graphics.Core.Helpers;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe class PhysicalDevice : VulkanObject<VkPhysicalDevice>
{
    internal PhysicalDevice(VulkanResources vkRes, VkPhysicalDevice physicalDevice) : base(vkRes, ObjectType.PhysicalDevice)
    {
        PhysicalDeviceProperties2 properties2 = new()
        {
            SType = StructureType.PhysicalDeviceProperties2
        };

        properties2.AddNext(out PhysicalDeviceDescriptorIndexingProperties descriptorIndexingProperties)
                   .AddNext(out PhysicalDeviceDescriptorBufferPropertiesEXT descriptorBufferProperties)
                   .AddNext(out PhysicalDeviceRayTracingPipelinePropertiesKHR rayTracingPipelineProperties);

        VkRes.Vk.GetPhysicalDeviceProperties2(physicalDevice, &properties2);

        PhysicalDeviceFeatures features;
        VkRes.Vk.GetPhysicalDeviceFeatures(physicalDevice, &features);

        PhysicalDeviceMemoryProperties memoryProperties;
        VkRes.Vk.GetPhysicalDeviceMemoryProperties(physicalDevice, &memoryProperties);

        uint queueFamilyPropertyCount = 0;
        VkRes.Vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamilyPropertyCount, null);

        QueueFamilyProperties[] queueFamilyProperties = new QueueFamilyProperties[(int)queueFamilyPropertyCount];
        VkRes.Vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamilyPropertyCount, queueFamilyProperties);

        uint extensionPropertyCount = 0;
        VkRes.Vk.EnumerateDeviceExtensionProperties(physicalDevice, string.Empty, &extensionPropertyCount, null);

        ExtensionProperties[] extensionProperties = new ExtensionProperties[(int)extensionPropertyCount];
        VkRes.Vk.EnumerateDeviceExtensionProperties(physicalDevice, string.Empty, &extensionPropertyCount, extensionProperties);

        // Prevent invalid stack memory access.
        properties2.PNext = null;

        Handle = physicalDevice;
        Name = Alloter.GetString(properties2.Properties.DeviceName);
        Properties2 = properties2;
        DescriptorIndexingProperties = descriptorIndexingProperties;
        DescriptorBufferProperties = descriptorBufferProperties;
        RayTracingPipelineProperties = rayTracingPipelineProperties;
        Features = features;
        MemoryProperties = memoryProperties;
        QueueFamilyProperties = queueFamilyProperties;
        ExtensionProperties = extensionProperties;
        DescriptorBufferSupported = SupportsExtension(ExtDescriptorBuffer.ExtensionName);
        RayQuerySupported = SupportsExtension("VK_KHR_ray_query");
        RayTracingSupported = SupportsExtension(KhrRayTracingPipeline.ExtensionName);
        Score = CalculateScore();
    }

    internal override VkPhysicalDevice Handle { get; }

    internal PhysicalDeviceProperties2 Properties2 { get; }

    internal PhysicalDeviceDescriptorIndexingProperties DescriptorIndexingProperties { get; }

    internal PhysicalDeviceDescriptorBufferPropertiesEXT DescriptorBufferProperties { get; }

    internal PhysicalDeviceRayTracingPipelinePropertiesKHR RayTracingPipelineProperties { get; }

    internal PhysicalDeviceFeatures Features { get; }

    internal PhysicalDeviceMemoryProperties MemoryProperties { get; }

    internal QueueFamilyProperties[] QueueFamilyProperties { get; }

    internal ExtensionProperties[] ExtensionProperties { get; }

    internal bool DescriptorBufferSupported { get; }

    internal bool RayQuerySupported { get; }

    internal bool RayTracingSupported { get; }

    public uint Score { get; }

    internal uint GetQueueFamilyIndex(QueueFlags flags)
    {
        for (int i = 0; i < QueueFamilyProperties.Length; i++)
        {
            if ((QueueFamilyProperties[i].QueueFlags & flags) == flags)
            {
                return (uint)i;
            }
        }

        return 0;
    }

    internal uint FindMemoryTypeIndex(uint memoryTypeBits, MemoryPropertyFlags memoryPropertyFlags)
    {
        for (uint i = 0; i < MemoryProperties.MemoryTypeCount; i++)
        {
            if ((memoryTypeBits & (1 << (int)i)) != 0 && (MemoryProperties.MemoryTypes[(int)i].PropertyFlags & memoryPropertyFlags) == memoryPropertyFlags)
            {
                return i;
            }
        }

        throw new InvalidOperationException("Failed to find memory type index!");
    }

    internal Format FindSupportedFormat(Format[] candidates, ImageTiling tiling, FormatFeatureFlags features)
    {
        foreach (Format format in candidates)
        {
            FormatProperties formatProperties;
            VkRes.Vk.GetPhysicalDeviceFormatProperties(Handle, format, &formatProperties);

            if (tiling == ImageTiling.Linear && formatProperties.LinearTilingFeatures.HasFlag(features))
            {
                return format;
            }

            if (tiling == ImageTiling.Optimal && formatProperties.OptimalTilingFeatures.HasFlag(features))
            {
                return format;
            }
        }

        throw new InvalidOperationException("Failed to find supported format!");
    }

    internal bool SupportsExtension(string extensionName)
    {
        foreach (ExtensionProperties extensionProperty in ExtensionProperties)
        {
            if (Alloter.GetString(extensionProperty.ExtensionName) == extensionName)
            {
                return true;
            }
        }

        return false;
    }

    internal uint CalculateScore()
    {
        uint score = 0;

        if (Properties2.Properties.DeviceType is PhysicalDeviceType.IntegratedGpu)
        {
            score += 1000;
        }

        if (Properties2.Properties.DeviceType is PhysicalDeviceType.DiscreteGpu)
        {
            score += 2000;
        }

        if (Properties2.Properties.DeviceType is PhysicalDeviceType.VirtualGpu)
        {
            score += 1500;
        }

        if (Properties2.Properties.DeviceType is PhysicalDeviceType.Cpu)
        {
            score += 500;
        }

        if (Features.GeometryShader)
        {
            score += 1000;
        }

        if (Features.TessellationShader)
        {
            score += 1000;
        }

        if (Features.ShaderInt16)
        {
            score += 1000;
        }

        if (Features.ShaderInt64)
        {
            score += 1000;
        }

        if (Features.ShaderFloat64)
        {
            score += 1000;
        }

        if (Features.SparseBinding)
        {
            score += 1000;
        }

        if (Features.SparseResidencyBuffer)
        {
            score += 1000;
        }

        if (Features.SparseResidencyImage2D)
        {
            score += 1000;
        }

        if (Features.SparseResidencyImage3D)
        {
            score += 1000;
        }

        if (Features.SparseResidency2Samples)
        {
            score += 1000;
        }

        if (Features.SparseResidency4Samples)
        {
            score += 1000;
        }

        if (Features.SparseResidency8Samples)
        {
            score += 1000;
        }

        if (Features.SparseResidency16Samples)
        {
            score += 1000;
        }

        if (Features.SparseResidencyAliased)
        {
            score += 1000;
        }

        if (Features.VariableMultisampleRate)
        {
            score += 1000;
        }

        if (Features.InheritedQueries)
        {
            score += 1000;
        }

        if (DescriptorBufferSupported)
        {
            score += 1000;
        }

        if (RayQuerySupported)
        {
            score += 1000;
        }

        if (RayTracingSupported)
        {
            score += 1000;
        }

        return score;
    }

    internal override ulong[] GetHandles()
    {
        return [(ulong)Handle.Handle];
    }

    protected override void Destroy()
    {
    }
}
