using Graphics.Core;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe partial class Context : DisposableObject
{
    public const string ValidationLayerName = "VK_LAYER_KHRONOS_validation";

#if DEBUG
    public const bool EnableValidationLayers = true;
#else
    public const bool EnableValidationLayers = false;
#endif

    private readonly StringAlloter _stringAlloter = new();

    private readonly Vk _vk;
    private readonly Instance _instance;

    public Context()
    {
        _vk = Vk.GetApi();

        // Create instance
        _instance = CreateInstance();
    }

    internal Vk Vk => _vk;

    protected override void Destroy()
    {
        _vk.Dispose();

        _stringAlloter.Dispose();
    }

    private Instance CreateInstance()
    {
        ApplicationInfo applicationInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)_stringAlloter.Allocate("Graphics"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)_stringAlloter.Allocate("Graphics"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version13
        };

        InstanceCreateInfo instanceCreateInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &applicationInfo
        };

        if (EnableValidationLayers && !CheckValidationLayerSupport())
        {
            throw new InvalidOperationException("Validation layers requested, but not available!");
        }

        if (EnableValidationLayers)
        {
            instanceCreateInfo.EnabledLayerCount = 1;
            instanceCreateInfo.PpEnabledLayerNames = (byte**)_stringAlloter.Allocate([ValidationLayerName]);
        }

        string[] extensions = [KhrSwapchain.ExtensionName];

        if (EnableValidationLayers)
        {
            extensions = [.. extensions, ExtDebugUtils.ExtensionName];
        }

        instanceCreateInfo.EnabledExtensionCount = (uint)extensions.Length;
        instanceCreateInfo.PpEnabledExtensionNames = (byte**)_stringAlloter.Allocate(extensions);

        Instance instance;

        Result result = _vk.CreateInstance(&instanceCreateInfo, null, &instance);
        if (result != Result.Success)
        {
            throw new InvalidOperationException("Failed to create instance!");
        }

        return instance;
    }

    private bool CheckValidationLayerSupport()
    {
        uint layerCount = 0;
        _vk.EnumerateInstanceLayerProperties(&layerCount, null);

        LayerProperties* availableLayers = stackalloc LayerProperties[(int)layerCount];
        _vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

        for (int i = 0; i < layerCount; i++)
        {
            if (StringAlloter.GetString(availableLayers[i].LayerName) == ValidationLayerName)
            {
                return true;
            }
        }

        return false;
    }
}
