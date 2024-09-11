using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class PhysicalDevice : ContextObject
{
    private readonly VkPhysicalDevice _vkPhysicalDevice;
    private readonly PhysicalDeviceProperties _properties;
    private readonly PhysicalDeviceDescriptorBufferPropertiesEXT _descriptorBufferProperties;
    private readonly PhysicalDeviceFeatures _features;
    private readonly PhysicalDeviceMemoryProperties _memoryProperties;
    private readonly QueueFamilyProperties[] _queueFamilyProperties;
    private readonly ExtensionProperties[] _extensionProperties;
    private readonly string _name;
    private readonly uint _score;

    internal PhysicalDevice(Context context, VkPhysicalDevice vkPhysicalDevice) : base(context)
    {
        PhysicalDeviceDescriptorBufferPropertiesEXT descriptorBufferProperties = new()
        {
            SType = StructureType.PhysicalDeviceDescriptorBufferPropertiesExt
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

        _vkPhysicalDevice = vkPhysicalDevice;
        _properties = properties2.Properties;
        _descriptorBufferProperties = descriptorBufferProperties;
        _features = features;
        _memoryProperties = memoryProperties;
        _queueFamilyProperties = queueFamilyProperties;
        _extensionProperties = extensionProperties;
        _name = Alloter.GetString(properties2.Properties.DeviceName);
        _score = properties2.Properties.DeviceType == PhysicalDeviceType.DiscreteGpu ? 1000u : 0u;
    }

    public string Name => _name;

    public uint Score => _score;

    internal VkPhysicalDevice VkPhysicalDevice => _vkPhysicalDevice;

    internal PhysicalDeviceProperties Properties => _properties;

    internal PhysicalDeviceDescriptorBufferPropertiesEXT DescriptorBufferProperties => _descriptorBufferProperties;

    internal PhysicalDeviceFeatures Features => _features;

    internal PhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;

    internal QueueFamilyProperties[] QueueFamilyProperties => _queueFamilyProperties;

    internal ExtensionProperties[] ExtensionProperties => _extensionProperties;

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
            Vk.GetPhysicalDeviceFormatProperties(_vkPhysicalDevice, format, &formatProperties);

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
        _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, null).ThrowCode("Failed to enumerate physical devices!");

        if (physicalDeviceCount == 0)
        {
            throw new InvalidOperationException("No physical devices found!");
        }

        // Enumerate physical devices
        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[(int)physicalDeviceCount];
        _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, physicalDevices).ThrowCode("Failed to enumerate physical devices!");

        return physicalDevices.Select(item => new PhysicalDevice(this, item)).ToArray();
    }

    public PhysicalDevice GetBestPhysicalDevice()
    {
        PhysicalDevice[] physicalDevices = EnumeratePhysicalDevices();

        return physicalDevices.OrderByDescending(item => item.Score).First();
    }
}
