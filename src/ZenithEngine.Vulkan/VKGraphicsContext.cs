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

    public override DeviceCapabilities Capabilities { get; }

    public override ResourceFactory Factory { get; }

    public override void CreateDeviceInternal(bool useDebugLayer)
    {
        if (Instance.Handle != 0)
        {
            return;
        }

        CreateInstance(useDebugLayer);
    }

    public override MappedResource MapMemory(Buffer buffer, MapMode mode)
    {
        throw new NotImplementedException();
    }

    public override void UnmapMemory(Buffer buffer)
    {
        throw new NotImplementedException();
    }

    protected override void DestroyInternal()
    {
        KhrSurface?.Dispose();
        Debug?.Dispose();

        Vk.DestroyInstance(Instance, null);

        Vk.Dispose();
    }

    private void CreateInstance(bool useDebugLayer)
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
}
