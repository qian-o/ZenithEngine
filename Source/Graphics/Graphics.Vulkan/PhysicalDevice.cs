using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class PhysicalDevice : DisposableObject
{
    private readonly Context _context;
    private readonly VkPhysicalDevice _vkPhysicalDevice;

    internal PhysicalDevice(Context context, VkPhysicalDevice vkPhysicalDevice)
    {
        _context = context;
        _vkPhysicalDevice = vkPhysicalDevice;

        PhysicalDeviceProperties physicalDeviceProperties;
        _context.Vk.GetPhysicalDeviceProperties(_vkPhysicalDevice, &physicalDeviceProperties);

        Name = StringAlloter.GetString(physicalDeviceProperties.DeviceName);
    }

    public string Name { get; }

    protected override void Destroy()
    {
    }
}

public unsafe partial class Context
{
    public PhysicalDevice[] GetPhysicalDevices()
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
        VkPhysicalDevice* physicalDevices = stackalloc VkPhysicalDevice[(int)physicalDeviceCount];
        result = _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, physicalDevices);

        if (result != Result.Success)
        {
            throw new InvalidOperationException("Failed to enumerate physical devices!");
        }

        PhysicalDevice[] devices = new PhysicalDevice[physicalDeviceCount];

        for (int i = 0; i < physicalDeviceCount; i++)
        {
            devices[i] = new PhysicalDevice(this, physicalDevices[i]);
        }

        return devices;
    }
}
