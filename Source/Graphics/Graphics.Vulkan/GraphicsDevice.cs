using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe class GraphicsDevice : ContextObject
{
    private readonly PhysicalDevice _physicalDevice;
    private readonly Device _device;
    private readonly KhrSwapchain _swapchainExt;
    private readonly SurfaceKHR _windowSurface;
    private readonly Queue _graphicsQueue;
    private readonly Queue _computeQueue;
    private readonly Queue _transferQueue;
    private readonly CommandPool _graphicsCommandPool;
    private readonly CommandPool _computeCommandPool;
    private readonly CommandPool _transferCommandPool;
    private readonly SwapChain _swapChain;

    internal GraphicsDevice(Context context,
                            PhysicalDevice physicalDevice,
                            Device device,
                            KhrSwapchain swapchainExt,
                            SurfaceKHR windowSurface,
                            uint graphicsQueueFamilyIndex,
                            uint computeQueueFamilyIndex,
                            uint transferQueueFamilyIndex) : base(context)
    {
        _physicalDevice = physicalDevice;
        _device = device;
        _swapchainExt = swapchainExt;
        _windowSurface = windowSurface;

        Queue graphicsQueue;
        Vk.GetDeviceQueue(device, graphicsQueueFamilyIndex, 0, &graphicsQueue);
        _graphicsQueue = graphicsQueue;

        Queue computeQueue;
        Vk.GetDeviceQueue(device, computeQueueFamilyIndex, 0, &computeQueue);
        _computeQueue = computeQueue;

        Queue transferQueue;
        Vk.GetDeviceQueue(device, transferQueueFamilyIndex, 0, &transferQueue);
        _transferQueue = transferQueue;

        CommandPool graphicsCommandPool;
        {
            CommandPoolCreateInfo commandPoolCreateInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = graphicsQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            Vk.CreateCommandPool(device, &commandPoolCreateInfo, null, &graphicsCommandPool);
        }
        _graphicsCommandPool = graphicsCommandPool;

        CommandPool computeCommandPool;
        {
            CommandPoolCreateInfo commandPoolCreateInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = computeQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            Vk.CreateCommandPool(device, &commandPoolCreateInfo, null, &computeCommandPool);
        }
        _computeCommandPool = computeCommandPool;

        CommandPool transferCommandPool;
        {
            CommandPoolCreateInfo commandPoolCreateInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = transferQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            Vk.CreateCommandPool(device, &commandPoolCreateInfo, null, &transferCommandPool);
        }
        _transferCommandPool = transferCommandPool;

        _swapChain = new SwapChain(context, this);
    }

    public PhysicalDevice PhysicalDevice => _physicalDevice;

    public SwapChain SwapChain => _swapChain;

    internal Device Device => _device;

    internal KhrSwapchain SwapchainExt => _swapchainExt;

    internal SurfaceKHR WindowSurface => _windowSurface;

    internal Queue GraphicsQueue => _graphicsQueue;

    internal Queue ComputeQueue => _computeQueue;

    internal Queue TransferQueue => _transferQueue;

    internal CommandPool GraphicsCommandPool => _graphicsCommandPool;

    internal CommandPool ComputeCommandPool => _computeCommandPool;

    internal CommandPool TransferCommandPool => _transferCommandPool;

    protected override void Destroy()
    {
        Vk.DestroyCommandPool(_device, _transferCommandPool, null);
        Vk.DestroyCommandPool(_device, _computeCommandPool, null);
        Vk.DestroyCommandPool(_device, _graphicsCommandPool, null);

        _swapchainExt.Dispose();

        Vk.DestroyDevice(_device, null);
    }
}

public unsafe partial class Context
{
    public GraphicsDevice CreateGraphicsDevice(PhysicalDevice physicalDevice, Window window)
    {
        float queuePriority = 1.0f;

        uint graphicsQueueFamilyIndex = GetQueueFamilyIndex(physicalDevice, QueueFlags.GraphicsBit);
        uint computeQueueFamilyIndex = GetQueueFamilyIndex(physicalDevice, QueueFlags.ComputeBit);
        uint transferQueueFamilyIndex = GetQueueFamilyIndex(physicalDevice, QueueFlags.TransferBit);

        HashSet<uint> uniqueQueueFamilyIndices =
        [
            graphicsQueueFamilyIndex,
            computeQueueFamilyIndex,
            transferQueueFamilyIndex
        ];

        DeviceQueueCreateInfo[] deviceQueueCreateInfos = new DeviceQueueCreateInfo[uniqueQueueFamilyIndices.Count];

        for (int i = 0; i < deviceQueueCreateInfos.Length; i++)
        {
            deviceQueueCreateInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueQueueFamilyIndices.ElementAt(i),
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };
        }

        string[] deviceExtensions = [KhrSwapchain.ExtensionName];

        PhysicalDeviceFeatures physicalDeviceFeatures = new()
        {
            SampleRateShading = Vk.True
        };

        DeviceCreateInfo deviceCreateInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)deviceQueueCreateInfos.Length,
            PQueueCreateInfos = _alloter.Allocate(deviceQueueCreateInfos),
            EnabledExtensionCount = (uint)deviceExtensions.Length,
            PpEnabledExtensionNames = _alloter.Allocate(deviceExtensions),
            PEnabledFeatures = &physicalDeviceFeatures
        };

        Device device;
        if (_vk.CreateDevice(physicalDevice.VkPhysicalDevice, &deviceCreateInfo, null, &device) != Result.Success)
        {
            throw new InvalidOperationException("Failed to create device.");
        }

        KhrSwapchain swapchainExt = CreateDeviceExtension<KhrSwapchain>(device)!;

        SurfaceKHR windowSurface = window.IWindow.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();

        _alloter.Clear();

        return new GraphicsDevice(this,
                                  physicalDevice,
                                  device,
                                  swapchainExt,
                                  windowSurface,
                                  graphicsQueueFamilyIndex,
                                  computeQueueFamilyIndex,
                                  transferQueueFamilyIndex);
    }

    private T? CreateDeviceExtension<T>(Device device) where T : NativeExtension<Vk>
    {
        if (!_vk.TryGetDeviceExtension(_instance, device, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }

    private static uint GetQueueFamilyIndex(PhysicalDevice physicalDevice, QueueFlags flags)
    {
        for (int i = 0; i < physicalDevice.QueueFamilyProperties.Length; i++)
        {
            if ((physicalDevice.QueueFamilyProperties[i].QueueFlags & flags) == flags)
            {
                return (uint)i;
            }
        }

        return 0;
    }
}
