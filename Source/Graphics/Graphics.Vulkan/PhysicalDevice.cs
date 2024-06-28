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
        _vkPhysicalDevice = vkPhysicalDevice;

        PhysicalDeviceProperties properties;
        Vk.GetPhysicalDeviceProperties(_vkPhysicalDevice, &properties);
        _properties = properties;

        PhysicalDeviceFeatures features;
        Vk.GetPhysicalDeviceFeatures(_vkPhysicalDevice, &features);
        _features = features;

        PhysicalDeviceMemoryProperties memoryProperties;
        Vk.GetPhysicalDeviceMemoryProperties(_vkPhysicalDevice, &memoryProperties);
        _memoryProperties = memoryProperties;

        uint queueFamilyPropertyCount = 0;
        Vk.GetPhysicalDeviceQueueFamilyProperties(_vkPhysicalDevice, &queueFamilyPropertyCount, null);

        _queueFamilyProperties = new QueueFamilyProperties[(int)queueFamilyPropertyCount];
        Vk.GetPhysicalDeviceQueueFamilyProperties(_vkPhysicalDevice, &queueFamilyPropertyCount, _queueFamilyProperties);

        uint extensionPropertyCount = 0;
        Vk.EnumerateDeviceExtensionProperties(_vkPhysicalDevice, string.Empty, &extensionPropertyCount, null);

        _extensionProperties = new ExtensionProperties[(int)extensionPropertyCount];
        Vk.EnumerateDeviceExtensionProperties(_vkPhysicalDevice, string.Empty, &extensionPropertyCount, _extensionProperties);

        _name = Alloter.GetString(properties.DeviceName);
    }

    public string Name => _name;

    internal VkPhysicalDevice VkPhysicalDevice => _vkPhysicalDevice;

    internal PhysicalDeviceProperties Properties => _properties;

    internal PhysicalDeviceFeatures Features => _features;

    internal PhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;

    internal QueueFamilyProperties[] QueueFamilyProperties => _queueFamilyProperties;

    internal ExtensionProperties[] ExtensionProperties => _extensionProperties;

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
