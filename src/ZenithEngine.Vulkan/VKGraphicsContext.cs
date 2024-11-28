using Silk.NET.Core;
using Silk.NET.Vulkan;
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
        Capabilities = new();
        Factory = new(this);
        DescriptorSetAllocator = new(this);
    }

    public Vk Vk { get; }

    public VKDebug? Debug { get; private set; }

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

    public VKDescriptorSetAllocator DescriptorSetAllocator { get; }

    public override void CreateDeviceInternal(bool useDebugLayer)
    {
        if (Instance.Handle != 0)
        {
            return;
        }

        InitInstance(useDebugLayer);
        InitPhysicalDevice();
        InitDevice();
    }

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

    public void SetDebugName(ObjectType objectType, ulong handle, string name)
    {
        Debug?.SetObjectName(Device, objectType, handle, name);
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
        Debug?.Dispose();

        Vk.DestroyInstance(Instance, null);

        Vk.Dispose();

        Debug = null;
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
            ApiVersion = (Version32)VulkanApiVersion
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

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = allocator.AllocUTF8(extensions);

        Vk.CreateInstance(&createInfo, null, out Instance).ThrowIfError();

        Debug = useDebugLayer ? new(this) : null;
        KhrSurface = Vk.TryGetExtension<KhrSurface>(Instance);
        KhrWin32Surface = Vk.TryGetExtension<KhrWin32Surface>(Instance);
        KhrWaylandSurface = Vk.TryGetExtension<KhrWaylandSurface>(Instance);
        KhrXlibSurface = Vk.TryGetExtension<KhrXlibSurface>(Instance);
        KhrAndroidSurface = Vk.TryGetExtension<KhrAndroidSurface>(Instance);
        MvkIosSurface = Vk.TryGetExtension<MvkIosSurface>(Instance);
        MvkMacosSurface = Vk.TryGetExtension<MvkMacosSurface>(Instance);
    }
}
