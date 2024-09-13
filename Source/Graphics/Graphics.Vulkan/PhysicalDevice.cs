using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class PhysicalDevice : ContextObject
{
    internal PhysicalDevice(Context context, VkPhysicalDevice vkPhysicalDevice) : base(context)
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
        Vk.GetPhysicalDeviceProperties2(vkPhysicalDevice, &properties2);

        PhysicalDeviceFeatures features;
        Vk.GetPhysicalDeviceFeatures(vkPhysicalDevice, &features);

        PhysicalDeviceMemoryProperties memoryProperties;
        Vk.GetPhysicalDeviceMemoryProperties(vkPhysicalDevice, &memoryProperties);

        uint queueFamilyPropertyCount = 0;
        Vk.GetPhysicalDeviceQueueFamilyProperties(vkPhysicalDevice, &queueFamilyPropertyCount, null);

        QueueFamilyProperties[] queueFamilyProperties = new QueueFamilyProperties[(int)queueFamilyPropertyCount];
        Vk.GetPhysicalDeviceQueueFamilyProperties(vkPhysicalDevice, &queueFamilyPropertyCount, queueFamilyProperties);

        uint extensionPropertyCount = 0;
        Vk.EnumerateDeviceExtensionProperties(vkPhysicalDevice, string.Empty, &extensionPropertyCount, null);

        ExtensionProperties[] extensionProperties = new ExtensionProperties[(int)extensionPropertyCount];
        Vk.EnumerateDeviceExtensionProperties(vkPhysicalDevice, string.Empty, &extensionPropertyCount, extensionProperties);

        Handle = vkPhysicalDevice;
        Properties = properties2.Properties;
        DescriptorBufferProperties = descriptorBufferProperties;
        DescriptorIndexingProperties = descriptorIndexingProperties;
        Features = features;
        MemoryProperties = memoryProperties;
        QueueFamilyProperties = queueFamilyProperties;
        ExtensionProperties = extensionProperties;
        Name = Alloter.GetString(properties2.Properties.DeviceName);
        Score = properties2.Properties.DeviceType == PhysicalDeviceType.DiscreteGpu ? 1000u : 0u;
    }

    public string Name { get; }

    public uint Score { get; }

    internal VkPhysicalDevice Handle { get; }

    internal PhysicalDeviceProperties Properties { get; }

    internal PhysicalDeviceDescriptorBufferPropertiesEXT DescriptorBufferProperties { get; }

    internal PhysicalDeviceDescriptorIndexingProperties DescriptorIndexingProperties { get; }

    internal PhysicalDeviceFeatures Features { get; }

    internal PhysicalDeviceMemoryProperties MemoryProperties { get; }

    internal QueueFamilyProperties[] QueueFamilyProperties { get; }

    internal ExtensionProperties[] ExtensionProperties { get; }

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
            Vk.GetPhysicalDeviceFormatProperties(Handle, format, &formatProperties);

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

public unsafe partial class Context
{
    public PhysicalDevice[] EnumeratePhysicalDevices()
    {
        // Physical device
        uint physicalDeviceCount = 0;
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, null).ThrowCode("Failed to enumerate physical devices!");

        if (physicalDeviceCount == 0)
        {
            throw new InvalidOperationException("No physical devices found!");
        }

        // Enumerate physical devices
        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[(int)physicalDeviceCount];
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, physicalDevices).ThrowCode("Failed to enumerate physical devices!");

        return physicalDevices.Select(item => new PhysicalDevice(this, item)).ToArray();
    }

    public PhysicalDevice GetBestPhysicalDevice()
    {
        PhysicalDevice[] physicalDevices = EnumeratePhysicalDevices();

        return physicalDevices.OrderByDescending(item => item.Score).First();
    }
}
