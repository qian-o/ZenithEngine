using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Vulkan.Extensions.MVK;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe partial class VKGraphicsContext : GraphicsContext
{
    public VkInstance Instance;

    public VKGraphicsContext()
    {
        Vk = Vk.GetApi();
        Backend = Backend.Vulkan;
        Capabilities = new(this);
        Factory = new(this);
    }

    public Vk Vk { get; }

    public VKDebugLayer? DebugLayer { get; private set; }

    public ExtDebugUtils? ExtDebugUtils { get; private set; }

    public KhrSurface? KhrSurface { get; private set; }

    public KhrWin32Surface? KhrWin32Surface { get; private set; }

    public KhrWaylandSurface? KhrWaylandSurface { get; private set; }

    public KhrXlibSurface? KhrXlibSurface { get; private set; }

    public KhrAndroidSurface? KhrAndroidSurface { get; private set; }

    public MvkIosSurface? MvkIosSurface { get; private set; }

    public MvkMacosSurface? MvkMacosSurface { get; private set; }

    public override Backend Backend { get; }

    public override VKDeviceCapabilities Capabilities { get; }

    public override VKResourceFactory Factory { get; }

    public override MappedResource MapMemory(Buffer buffer, MapMode mode)
    {
        void* data;
        Vk.MapMemory(Device,
                     buffer.VK().DeviceMemory.DeviceMemory,
                     0,
                     buffer.Desc.SizeInBytes,
                     0,
                     &data).ThrowIfError();

        return new(buffer, mode, (nint)data, buffer.Desc.SizeInBytes);
    }

    public override void UnmapMemory(Buffer buffer)
    {
        Vk.UnmapMemory(Device, buffer.VK().DeviceMemory.DeviceMemory);
    }

    protected override void CreateDeviceInternal(bool useDebugLayer)
    {
        if (Instance.Handle is not 0)
        {
            return;
        }

        InitInstance(useDebugLayer);
        InitPhysicalDevice();
        InitDevice();
    }

    protected override void DestroyInternal()
    {
        DestroyDevice();

        MvkMacosSurface?.Dispose();
        MvkIosSurface?.Dispose();
        KhrAndroidSurface?.Dispose();
        KhrXlibSurface?.Dispose();
        KhrWaylandSurface?.Dispose();
        KhrWin32Surface?.Dispose();
        KhrSurface?.Dispose();
        DebugLayer?.Dispose();

        Vk.DestroyInstance(Instance, null);

        Vk.Dispose();

        DebugLayer = null;
        KhrSurface = null;
        KhrWin32Surface = null;
        KhrWaylandSurface = null;
        KhrXlibSurface = null;
        KhrAndroidSurface = null;
        MvkIosSurface = null;
        MvkMacosSurface = null;
    }

    private void InitInstance(bool useDebugLayer)
    {
        using MemoryAllocator allocator = new();

        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = allocator.AllocUTF8(AppDomain.CurrentDomain.FriendlyName),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = allocator.AllocUTF8("Zenith Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = new Version32(1, 3, 0)
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            PpEnabledExtensionNames = InstanceExtensions(allocator, out uint extensionCount),
            EnabledExtensionCount = extensionCount
        };

        if (useDebugLayer)
        {
            const string ValidationLayerName = "VK_LAYER_KHRONOS_validation";

            uint layerCount;
            Vk.EnumerateInstanceLayerProperties(&layerCount, null).ThrowIfError();

            LayerProperties[] layers = new LayerProperties[(int)layerCount];
            Vk.EnumerateInstanceLayerProperties(&layerCount, layers).ThrowIfError();

            bool layerFound = false;

            foreach (LayerProperties layer in layers)
            {
                if (ValidationLayerName == Utils.PtrToStringUTF8((nint)layer.LayerName))
                {
                    layerFound = true;

                    break;
                }
            }

            if (!layerFound)
            {
                throw new ZenithEngineException("Validation layer not found.");
            }

            createInfo.EnabledLayerCount = 1;
            createInfo.PpEnabledLayerNames = allocator.AllocUTF8([ValidationLayerName]);
        }

        Vk.CreateInstance(&createInfo, null, out Instance).ThrowIfError();

        DebugLayer = useDebugLayer ? new(this) : null;
        ExtDebugUtils = Vk.TryGetExtension<ExtDebugUtils>(Instance);
        KhrSurface = Vk.TryGetExtension<KhrSurface>(Instance);
        KhrWin32Surface = Vk.TryGetExtension<KhrWin32Surface>(Instance);
        KhrWaylandSurface = Vk.TryGetExtension<KhrWaylandSurface>(Instance);
        KhrXlibSurface = Vk.TryGetExtension<KhrXlibSurface>(Instance);
        KhrAndroidSurface = Vk.TryGetExtension<KhrAndroidSurface>(Instance);
        MvkIosSurface = Vk.TryGetExtension<MvkIosSurface>(Instance);
        MvkMacosSurface = Vk.TryGetExtension<MvkMacosSurface>(Instance);
    }

    private static byte** InstanceExtensions(MemoryAllocator allocator, out uint count)
    {
        string[] extensions = [ExtDebugUtils.ExtensionName, KhrSurface.ExtensionName];

        if (OperatingSystem.IsWindows())
        {
            extensions = [.. extensions, KhrWin32Surface.ExtensionName];
        }
        else if (OperatingSystem.IsLinux())
        {
            extensions = [.. extensions, KhrWaylandSurface.ExtensionName, KhrXlibSurface.ExtensionName];
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

        count = (uint)extensions.Length;

        return allocator.AllocUTF8(extensions);
    }
}
