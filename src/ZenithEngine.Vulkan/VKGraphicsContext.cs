using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Vulkan.Extensions.MVK;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKGraphicsContext : GraphicsContext
{
    public static readonly Version32 Version = Vk.Version13;

    public VkInstance Instance;

    public VkPhysicalDevice PhysicalDevice;

    public VKGraphicsContext()
    {
        Vk = Vk.GetApi();
        Backend = Backend.Vulkan;
        Capabilities = new VKDeviceCapabilities();
        Factory = new VKResourceFactory(this);
    }

    public Vk Vk { get; }

    public VKDebug? Debug { get; private set; }

    public KhrSurface? KhrSurface { get; private set; }

    public override Backend Backend { get; }

    public override VKDeviceCapabilities Capabilities { get; }

    public override VKResourceFactory Factory { get; }

    public override void CreateDeviceInternal(bool useDebugLayer)
    {
        if (Instance.Handle != 0)
        {
            return;
        }

        InitInstance(useDebugLayer);
        InitPhysicalDevice();
    }

    public override MappedResource MapMemory(Buffer buffer, MapMode mode)
    {
        throw new NotImplementedException();
    }

    public override void UnmapMemory(Buffer buffer)
    {
        throw new NotImplementedException();
    }

    public uint FindMemoryTypeIndex(uint typeFilter, MemoryPropertyFlags flags)
    {
        PhysicalDeviceMemoryProperties memoryProperties;
        Vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, &memoryProperties);

        for (int i = 0; i < memoryProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1 << i)) != 0 && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(flags))
            {
                return (uint)i;
            }
        }

        throw new ZenithEngineException(Backend, "Failed to find suitable memory type.");
    }

    protected override void DestroyInternal()
    {
        KhrSurface?.Dispose();
        Debug?.Dispose();

        Vk.DestroyInstance(Instance, null);

        Vk.Dispose();
    }

    private void InitInstance(bool useDebugLayer)
    {
        using MemoryAllocator allocator = new();

        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)allocator.AllocAnsi(AppDomain.CurrentDomain.FriendlyName),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)allocator.AllocAnsi("Zenith Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Version
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        if (useDebugLayer)
        {
            const string ValidationLayerName = "VK_LAYER_KHRONOS_validation";

            uint layerCount = 0;
            Vk.EnumerateInstanceLayerProperties(&layerCount, null);

            LayerProperties[] layers = new LayerProperties[(int)layerCount];
            Vk.EnumerateInstanceLayerProperties(&layerCount, layers);

            bool layerFound = false;

            foreach (LayerProperties layer in layers)
            {
                if (ValidationLayerName == Utils.PtrToStringAnsi((nint)layer.LayerName))
                {
                    layerFound = true;

                    break;
                }
            }

            if (!layerFound)
            {
                throw new ZenithEngineException(Backend, "Validation layer not found.");
            }

            createInfo.EnabledLayerCount = 1;
            createInfo.PpEnabledLayerNames = (byte**)allocator.AllocAnsi([ValidationLayerName]);
        }

        string[] extensions = [KhrSurface.ExtensionName];

        if (useDebugLayer)
        {
            extensions = [.. extensions, .. VKDebug.ExtensionNames];
        }

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
        else if (OperatingSystem.IsIOS())
        {
            extensions = [.. extensions, MvkIosSurface.ExtensionName];
        }
        else if (OperatingSystem.IsMacOS())
        {
            extensions = [.. extensions, MvkMacosSurface.ExtensionName];
        }

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = (byte**)allocator.AllocAnsi(extensions);

        Vk.CreateInstance(&createInfo, null, out Instance).ThrowIfError();

        Debug = useDebugLayer ? new VKDebug(this) : null;
        KhrSurface = Vk.GetExtension<KhrSurface>(Instance);
    }

    private void InitPhysicalDevice()
    {
        uint physicalDeviceCount = 0;
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, null);

        if (physicalDeviceCount == 0)
        {
            throw new ZenithEngineException(Backend, "No physical devices found.");
        }

        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[physicalDeviceCount];
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, physicalDevices).ThrowIfError();

        uint bestScore = 0;
        VkPhysicalDevice bestPhysicalDevice = default;

        foreach (VkPhysicalDevice physicalDevice in physicalDevices)
        {
            uint score = CalcPhysicalDeviceScore(physicalDevice);

            if (score > bestScore)
            {
                bestScore = score;
                bestPhysicalDevice = physicalDevice;
            }
        }

        PhysicalDevice = bestPhysicalDevice;
        Capabilities.Init(this);
    }

    private uint CalcPhysicalDeviceScore(VkPhysicalDevice physicalDevice)
    {
        PhysicalDeviceFeatures features;
        Vk.GetPhysicalDeviceFeatures(physicalDevice, &features);

        PhysicalDeviceProperties properties;
        Vk.GetPhysicalDeviceProperties(physicalDevice, &properties);

        uint score = 0;

        if (features.GeometryShader)
        {
            score += 1000;
        }

        if (features.TessellationShader)
        {
            score += 1000;
        }

        if (features.ShaderInt16)
        {
            score += 1000;
        }

        if (features.ShaderInt64)
        {
            score += 1000;
        }

        if (features.ShaderFloat64)
        {
            score += 1000;
        }

        if (features.SparseBinding)
        {
            score += 1000;
        }

        if (features.SparseResidencyBuffer)
        {
            score += 1000;
        }

        if (features.SparseResidencyImage2D)
        {
            score += 1000;
        }

        if (features.SparseResidencyImage3D)
        {
            score += 1000;
        }

        if (features.SparseResidency2Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency4Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency8Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency16Samples)
        {
            score += 1000;
        }

        if (features.SparseResidencyAliased)
        {
            score += 1000;
        }

        if (features.VariableMultisampleRate)
        {
            score += 1000;
        }

        if (features.InheritedQueries)
        {
            score += 1000;
        }

        score += properties.ApiVersion;

        if (properties.DeviceType is PhysicalDeviceType.IntegratedGpu)
        {
            score += 4000;
        }

        if (properties.DeviceType is PhysicalDeviceType.DiscreteGpu)
        {
            score += 6000;
        }

        if (properties.DeviceType is PhysicalDeviceType.VirtualGpu)
        {
            score += 2000;
        }

        if (properties.DeviceType is PhysicalDeviceType.Cpu)
        {
            score += 1000;
        }

        return score;
    }
}
