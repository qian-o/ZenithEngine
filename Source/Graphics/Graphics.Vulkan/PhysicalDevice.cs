using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class PhysicalDevice : VulkanObject<VkPhysicalDevice>
{
    internal PhysicalDevice(VulkanResources vkRes, VkPhysicalDevice physicalDevice) : base(vkRes, ObjectType.PhysicalDevice)
    {
        PhysicalDeviceDescriptorIndexingProperties descriptorIndexingProperties = new()
        {
            SType = StructureType.PhysicalDeviceDescriptorIndexingProperties
        };
        PhysicalDeviceDescriptorBufferPropertiesEXT descriptorBufferProperties = new()
        {
            SType = StructureType.PhysicalDeviceDescriptorBufferPropertiesExt,
            PNext = &descriptorIndexingProperties
        };
        PhysicalDeviceProperties2 properties2 = new()
        {
            SType = StructureType.PhysicalDeviceProperties2,
            PNext = &descriptorBufferProperties
        };
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

        Handle = physicalDevice;
        Name = Alloter.GetString(properties2.Properties.DeviceName);
        Properties = properties2.Properties;
        DescriptorBufferProperties = descriptorBufferProperties;
        DescriptorIndexingProperties = descriptorIndexingProperties;
        Features = features;
        MemoryProperties = memoryProperties;
        QueueFamilyProperties = queueFamilyProperties;
        ExtensionProperties = extensionProperties;
        Score = properties2.Properties.DeviceType == PhysicalDeviceType.DiscreteGpu ? 1000u : 0u;
    }

    internal override VkPhysicalDevice Handle { get; }

    internal PhysicalDeviceProperties Properties { get; }

    internal PhysicalDeviceDescriptorBufferPropertiesEXT DescriptorBufferProperties { get; }

    internal PhysicalDeviceDescriptorIndexingProperties DescriptorIndexingProperties { get; }

    internal PhysicalDeviceFeatures Features { get; }

    internal PhysicalDeviceMemoryProperties MemoryProperties { get; }

    internal QueueFamilyProperties[] QueueFamilyProperties { get; }

    internal ExtensionProperties[] ExtensionProperties { get; }

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

    internal override ulong[] GetHandles()
    {
        return [(ulong)Handle.Handle];
    }

    protected override void Destroy()
    {
    }
}
