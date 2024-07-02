using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class PhysicalDevice : ContextObject
{
    private readonly VkPhysicalDevice _vkPhysicalDevice;
    private readonly PhysicalDeviceProperties _properties;
    private readonly PhysicalDeviceFeatures _features;
    private readonly PhysicalDeviceMemoryProperties _memoryProperties;
    private readonly QueueFamilyProperties[] _queueFamilyProperties;
    private readonly ExtensionProperties[] _extensionProperties;
    private readonly string _name;

    internal PhysicalDevice(Context context, VkPhysicalDevice vkPhysicalDevice) : base(context)
    {
        PhysicalDeviceProperties properties;
        Vk.GetPhysicalDeviceProperties(vkPhysicalDevice, &properties);

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
        _properties = properties;
        _features = features;
        _memoryProperties = memoryProperties;
        _queueFamilyProperties = queueFamilyProperties;
        _extensionProperties = extensionProperties;
        _name = Alloter.GetString(properties.DeviceName);
    }

    public string Name => _name;

    internal VkPhysicalDevice VkPhysicalDevice => _vkPhysicalDevice;

    internal PhysicalDeviceProperties Properties => _properties;

    internal PhysicalDeviceFeatures Features => _features;

    internal PhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;

    internal QueueFamilyProperties[] QueueFamilyProperties => _queueFamilyProperties;

    internal ExtensionProperties[] ExtensionProperties => _extensionProperties;

    public uint FindMemoryTypeIndex(uint memoryTypeBits, MemoryPropertyFlags memoryPropertyFlags)
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

    public Format FindSupportedFormat(Format[] candidates, ImageTiling tiling, FormatFeatureFlags features)
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
        Result result;

        // Physical device
        uint physicalDeviceCount = 0;
        result = _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, null);

        if (result != Result.Success)
        {
            throw new InvalidOperationException("Failed to enumerate physical devices!");
        }

        if (physicalDeviceCount == 0)
        {
            throw new InvalidOperationException("No physical devices found!");
        }

        // Enumerate physical devices
        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[(int)physicalDeviceCount];
        result = _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, physicalDevices);

        if (result != Result.Success)
        {
            throw new InvalidOperationException("Failed to enumerate physical devices!");
        }

        return physicalDevices.Select(item => new PhysicalDevice(this, item)).ToArray();
    }
}
