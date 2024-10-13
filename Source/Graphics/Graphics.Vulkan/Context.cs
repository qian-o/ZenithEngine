﻿using System.Globalization;
using System.Text;
using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Helpers;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe class Context : DisposableObject
{
    private const string ValidationLayerName = "VK_LAYER_KHRONOS_validation";

    private readonly Alloter _alloter;
    private readonly Vk _vk;
    private readonly VkInstance _instance;
    private readonly ExtDebugUtils? _debugUtils;
    private readonly KhrSurface _surface;
    private readonly DebugUtilsMessengerEXT? _debugUtilsMessenger;
    private readonly Dictionary<PhysicalDevice, VulkanResources> _physicalDeviceMap;
    private readonly Dictionary<GraphicsDevice, VulkanResources> _graphicsDeviceMap;

    static Context()
    {
        ApiVersion = Vk.Version13;

#if DEBUG
        Debugging = true;
#else
        Debugging = false;
#endif
    }

    public Context()
    {
        _alloter = new();
        _vk = Vk.GetApi();

        // Create instance
        _instance = CreateInstance();

        // Load instance extensions
        _debugUtils = Debugging ? CreateInstanceExtension<ExtDebugUtils>() : null;
        _surface = CreateInstanceExtension<KhrSurface>();

        // Debug message callback
        if (Debugging)
        {
            DebugUtilsMessengerCreateInfoEXT createInfo = new()
            {
                SType = StructureType.DebugUtilsMessengerCreateInfoExt,
                MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.InfoBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt
                              | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt
                              | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt
                              | DebugUtilsMessageTypeFlagsEXT.DeviceAddressBindingBitExt,
                PfnUserCallback = (PfnDebugUtilsMessengerCallbackEXT)DebugMessageCallback
            };

            DebugUtilsMessengerEXT debugUtilsMessenger;
            _debugUtils!.CreateDebugUtilsMessenger(_instance, &createInfo, null, &debugUtilsMessenger)
                        .ThrowCode("Failed to create debug messenger!");

            _debugUtilsMessenger = debugUtilsMessenger;
        }

        _physicalDeviceMap = [];
        _graphicsDeviceMap = [];
    }

    public static Version32 ApiVersion { get; }

    public static bool Debugging { get; }

    public PhysicalDevice[] EnumeratePhysicalDevices()
    {
        uint physicalDeviceCount = 0;
        _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, null).ThrowCode("Failed to enumerate physical devices!");

        if (physicalDeviceCount == 0)
        {
            throw new InvalidOperationException("No physical devices found!");
        }

        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[(int)physicalDeviceCount];
        _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, physicalDevices).ThrowCode("Failed to enumerate physical devices!");

        return physicalDevices.Select(Create).ToArray();

        PhysicalDevice Create(VkPhysicalDevice vkPhysicalDevice)
        {
            lock (_physicalDeviceMap)
            {
                VulkanResources vulkanResources = new();
                vulkanResources.InitializeContext(_vk, _instance, GetInstanceExtensions(), _debugUtils, _surface);

                PhysicalDevice physicalDevice = new(vulkanResources, vkPhysicalDevice);

                _physicalDeviceMap.Add(physicalDevice, vulkanResources);

                return physicalDevice;
            }
        }
    }

    public PhysicalDevice GetBestPhysicalDevice()
    {
        PhysicalDevice[] physicalDevices = EnumeratePhysicalDevices();

        return physicalDevices.OrderByDescending(item => item.Score).First();
    }

    public GraphicsDevice CreateGraphicsDevice(PhysicalDevice physicalDevice)
    {
        float queuePriority = 1.0f;

        uint graphicsQueueFamilyIndex = physicalDevice.GetQueueFamilyIndex(QueueFlags.GraphicsBit);
        uint computeQueueFamilyIndex = physicalDevice.GetQueueFamilyIndex(QueueFlags.ComputeBit);
        uint transferQueueFamilyIndex = physicalDevice.GetQueueFamilyIndex(QueueFlags.TransferBit);

        HashSet<uint> uniqueQueueFamilyIndices =
        [
            graphicsQueueFamilyIndex,
            computeQueueFamilyIndex,
            transferQueueFamilyIndex
        ];

        DeviceQueueCreateInfo[] createInfos = new DeviceQueueCreateInfo[uniqueQueueFamilyIndices.Count];

        for (int i = 0; i < createInfos.Length; i++)
        {
            createInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueQueueFamilyIndices.ElementAt(i),
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };
        }

        string[] extensions = GetDeviceExtensions(physicalDevice);

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)createInfos.Length,
            PQueueCreateInfos = _alloter.Allocate(createInfos),
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = _alloter.Allocate(extensions)
        };

        createInfo.AddNext(out PhysicalDeviceFeatures2 features2)
                  .AddNext(out PhysicalDeviceVulkan13Features _)
                  .AddNext(out PhysicalDeviceScalarBlockLayoutFeatures _)
                  .AddNext(out PhysicalDeviceDescriptorIndexingFeatures _)
                  .AddNext(out PhysicalDeviceBufferDeviceAddressFeatures _);

        if (physicalDevice.DescriptorBufferSupported)
        {
            createInfo.AddNext(out PhysicalDeviceDescriptorBufferFeaturesEXT _);
        }

        if (physicalDevice.RayQuerySupported)
        {
            createInfo.AddNext(out PhysicalDeviceRayQueryFeaturesKHR _);
        }

        if (physicalDevice.RayTracingSupported)
        {
            createInfo.AddNext(out PhysicalDeviceRayTracingPipelineFeaturesKHR _);
        }

        if (physicalDevice.RayQuerySupported || physicalDevice.RayTracingSupported)
        {
            createInfo.AddNext(out PhysicalDeviceAccelerationStructureFeaturesKHR _);
        }

        _vk.GetPhysicalDeviceFeatures2(physicalDevice.Handle, &features2);

        VkDevice device;
        _vk.CreateDevice(physicalDevice.Handle, &createInfo, null, &device).ThrowCode("Failed to create device.");

        _alloter.Clear();

        return Create(device);

        GraphicsDevice Create(VkDevice vkDevice)
        {
            lock (_graphicsDeviceMap)
            {
                VulkanResources vulkanResources = new();
                vulkanResources.InitializeContext(_vk, _instance, GetInstanceExtensions(), _debugUtils, _surface);
                vulkanResources.InitializePhysicalDevice(physicalDevice, GetDeviceExtensions(physicalDevice));

                GraphicsDevice graphicsDevice = new(vulkanResources,
                                                    vkDevice,
                                                    graphicsQueueFamilyIndex,
                                                    computeQueueFamilyIndex,
                                                    transferQueueFamilyIndex);

                _graphicsDeviceMap.Add(graphicsDevice, vulkanResources);

                return graphicsDevice;
            }
        }
    }

    protected override void Destroy()
    {
        foreach (KeyValuePair<PhysicalDevice, VulkanResources> pair in _physicalDeviceMap)
        {
            pair.Value.Dispose();
        }
        foreach (KeyValuePair<GraphicsDevice, VulkanResources> pair in _graphicsDeviceMap)
        {
            pair.Value.Dispose();
        }

        _physicalDeviceMap.Clear();
        _graphicsDeviceMap.Clear();

        _debugUtils?.DestroyDebugUtilsMessenger(_instance, _debugUtilsMessenger!.Value, null);
        _debugUtils?.Dispose();

        _surface.Dispose();

        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();

        _alloter.Dispose();
    }

    /// <summary>
    /// Check validation layer support.
    /// </summary>
    /// <returns></returns>
    private bool CheckValidationLayerSupport()
    {
        uint layerCount = 0;
        _vk.EnumerateInstanceLayerProperties(&layerCount, null);

        LayerProperties[] availableLayers = new LayerProperties[(int)layerCount];
        _vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

        uint extensionCount = 0;
        _vk.EnumerateInstanceExtensionProperties(string.Empty, &extensionCount, null);

        ExtensionProperties[] availableExtensions = new ExtensionProperties[(int)extensionCount];
        _vk.EnumerateInstanceExtensionProperties(string.Empty, &extensionCount, availableExtensions);

        bool layerFound = false;
        bool extensionFound = false;

        for (int i = 0; i < layerCount; i++)
        {
            LayerProperties layer = availableLayers[i];

            if (Alloter.GetString(layer.LayerName) == ValidationLayerName)
            {
                layerFound = true;
                break;
            }
        }

        for (int i = 0; i < extensionCount; i++)
        {
            ExtensionProperties extension = availableExtensions[i];

            if (Alloter.GetString(extension.ExtensionName) == ExtDebugUtils.ExtensionName)
            {
                extensionFound = true;
                break;
            }
        }

        return layerFound && extensionFound;
    }

    /// <summary>
    /// Create instance.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException</exception>
    private VkInstance CreateInstance()
    {
        ApplicationInfo applicationInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = _alloter.Allocate("Graphics"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = _alloter.Allocate("Graphics"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = ApiVersion
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &applicationInfo
        };

        if (Debugging && !CheckValidationLayerSupport())
        {
            throw new InvalidOperationException("Validation layers requested, but not available!");
        }

        if (Debugging)
        {
            createInfo.EnabledLayerCount = 1;
            createInfo.PpEnabledLayerNames = _alloter.Allocate([ValidationLayerName]);
        }

        string[] extensions = GetInstanceExtensions();

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = _alloter.Allocate(extensions);

        VkInstance instance;
        _vk.CreateInstance(&createInfo, null, &instance).ThrowCode("Failed to create instance!");

        _alloter.Clear();

        return instance;
    }

    /// <summary>
    /// Create instance extension.
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException</exception>
    private T CreateInstanceExtension<T>() where T : NativeExtension<Vk>
    {
        if (!_vk.TryGetInstanceExtension(_instance, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }

    private uint DebugMessageCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
                                      DebugUtilsMessageTypeFlagsEXT messageTypes,
                                      DebugUtilsMessengerCallbackDataEXT* pCallbackData,
                                      void* pUserData)
    {
        string message = Alloter.GetString(pCallbackData->PMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{messageSeverity}] [{messageTypes}]");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {Alloter.GetString(pCallbackData->PMessageIdName)}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Number: {pCallbackData->MessageIdNumber}");
        foreach (string str in strings)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{str}");
        }

        Console.ForegroundColor = messageSeverity switch
        {
            DebugUtilsMessageSeverityFlagsEXT.InfoBitExt => ConsoleColor.Blue,
            DebugUtilsMessageSeverityFlagsEXT.WarningBitExt => ConsoleColor.Yellow,
            DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine(stringBuilder);

        Console.ResetColor();

        return Vk.False;
    }

    private static string[] GetInstanceExtensions()
    {
        string[] extensions = [KhrSurface.ExtensionName];

        if (OperatingSystem.IsWindows())
        {
            extensions = [.. extensions, KhrWin32Surface.ExtensionName];
        }
        else if (OperatingSystem.IsLinux())
        {
            extensions = [.. extensions, KhrXlibSurface.ExtensionName];
        }
        else if (OperatingSystem.IsAndroid())
        {
            extensions = [.. extensions, KhrAndroidSurface.ExtensionName];
        }

        if (Debugging)
        {
            extensions = [.. extensions, ExtDebugUtils.ExtensionName];
        }

        return extensions;
    }

    private static string[] GetDeviceExtensions(PhysicalDevice physicalDevice)
    {
        string[] extensions = [KhrSwapchain.ExtensionName];

        if (physicalDevice.DescriptorBufferSupported)
        {
            extensions = [.. extensions, ExtDescriptorBuffer.ExtensionName];
        }

        if (physicalDevice.RayQuerySupported)
        {
            extensions = [.. extensions, "VK_KHR_ray_query"];
        }

        if (physicalDevice.RayTracingSupported)
        {
            extensions = [.. extensions, KhrRayTracingPipeline.ExtensionName];
        }

        if (physicalDevice.RayQuerySupported || physicalDevice.RayTracingSupported)
        {
            extensions = [.. extensions, KhrAccelerationStructure.ExtensionName, KhrDeferredHostOperations.ExtensionName];
        }

        return extensions;
    }
}
