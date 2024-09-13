using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class PhysicalDevice : VulkanObject<VkPhysicalDevice>
{
    private readonly PhysicalDeviceProperties _properties;
    private readonly PhysicalDeviceDescriptorBufferPropertiesEXT _descriptorBufferProperties;
    private readonly PhysicalDeviceDescriptorIndexingProperties _descriptorIndexingProperties;
    private readonly PhysicalDeviceFeatures _features;
    private readonly PhysicalDeviceMemoryProperties _memoryProperties;
    private readonly QueueFamilyProperties[] _queueFamilyProperties;
    private readonly ExtensionProperties[] _extensionProperties;

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
        Score = properties2.Properties.DeviceType == PhysicalDeviceType.DiscreteGpu ? 1000u : 0u;

        _properties = properties2.Properties;
        _descriptorBufferProperties = descriptorBufferProperties;
        _descriptorIndexingProperties = descriptorIndexingProperties;
        _features = features;
        _memoryProperties = memoryProperties;
        _queueFamilyProperties = queueFamilyProperties;
        _extensionProperties = extensionProperties;
    }

    internal override VkPhysicalDevice Handle { get; }

    public uint Score { get; }

    internal override ulong[] GetHandles()
    {
        return [(ulong)Handle.Handle];
    }

    internal void FillResources(VulkanResources vulkanResources)
    {
        vulkanResources.InitializePhysicalDevice(this,
                                                 _properties,
                                                 _descriptorBufferProperties,
                                                 _descriptorIndexingProperties,
                                                 _features,
                                                 _memoryProperties,
                                                 _queueFamilyProperties,
                                                 _extensionProperties);
    }

    internal uint GetQueueFamilyIndex(QueueFlags flags)
    {
        for (int i = 0; i < _queueFamilyProperties.Length; i++)
        {
            if ((_queueFamilyProperties[i].QueueFlags & flags) == flags)
            {
                return (uint)i;
            }
        }

        return 0;
    }

    internal uint FindMemoryTypeIndex(uint memoryTypeBits, MemoryPropertyFlags memoryPropertyFlags)
    {
        for (uint i = 0; i < _memoryProperties.MemoryTypeCount; i++)
        {
            if ((memoryTypeBits & (1 << (int)i)) != 0 && (_memoryProperties.MemoryTypes[(int)i].PropertyFlags & memoryPropertyFlags) == memoryPropertyFlags)
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

    protected override void Destroy()
    {
    }
}
