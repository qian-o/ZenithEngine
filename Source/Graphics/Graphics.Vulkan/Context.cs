using System.Runtime.InteropServices;
using Graphics.Core;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe partial class Context : DisposableObject
{
    public const string ValidationLayerName = "VK_LAYER_KHRONOS_validation";

    private readonly Alloter _alloter = new();

    private readonly Vk _vk;
    private readonly Instance _instance;
    private readonly ExtDebugUtils? _debugUtilsExt;
    private readonly KhrSurface _surfaceExt;

    static Context()
    {
#if DEBUG
        Debugging = true;
#else
        Debugging = false;
#endif
    }

    public Context()
    {
        _vk = Vk.GetApi();

        // Create instance
        _instance = CreateInstance();

        // Load instance extensions
        _debugUtilsExt = Debugging ? CreateInstanceExtension<ExtDebugUtils>() : null;
        _surfaceExt = CreateInstanceExtension<KhrSurface>()!;

        // Debug message callback
        if (Debugging)
        {
            DebugUtilsMessengerCreateInfoEXT debugUtilsMessengerCreateInfo = new()
            {
                SType = StructureType.DebugUtilsMessengerCreateInfoExt,
                MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt
                              | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt
                              | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                PfnUserCallback = (PfnDebugUtilsMessengerCallbackEXT)DebugMessageCallback
            };

            if (_debugUtilsExt!.CreateDebugUtilsMessenger(_instance, &debugUtilsMessengerCreateInfo, null, out _) != Result.Success)
            {
                throw new InvalidOperationException("Failed to set up debug messenger!");
            }
        }
    }

    internal Alloter Alloter => _alloter;

    internal Vk Vk => _vk;

    internal Instance Instance => _instance;

    internal KhrSurface SurfaceExt => _surfaceExt;

    public static bool Debugging { get; }

    protected override void Destroy()
    {
        _surfaceExt.Dispose();
        _debugUtilsExt?.Dispose();
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

        for (int i = 0; i < layerCount; i++)
        {
            LayerProperties layer = availableLayers[i];

            if (Alloter.GetString(layer.LayerName) == ValidationLayerName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Create instance.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException</exception>
    private Instance CreateInstance()
    {
        ApplicationInfo applicationInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = _alloter.Allocate("Graphics"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = _alloter.Allocate("Graphics"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version13
        };

        InstanceCreateInfo instanceCreateInfo = new()
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
            instanceCreateInfo.EnabledLayerCount = 1;
            instanceCreateInfo.PpEnabledLayerNames = _alloter.Allocate([ValidationLayerName]);
        }

        string[] extensions = [KhrSurface.ExtensionName];

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            extensions = [.. extensions, KhrWin32Surface.ExtensionName];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            extensions = [.. extensions, KhrXlibSurface.ExtensionName];
        }

        if (Debugging)
        {
            extensions = [.. extensions, ExtDebugUtils.ExtensionName];
        }

        instanceCreateInfo.EnabledExtensionCount = (uint)extensions.Length;
        instanceCreateInfo.PpEnabledExtensionNames = _alloter.Allocate(extensions);

        Instance instance;
        if (_vk.CreateInstance(&instanceCreateInfo, null, &instance) != Result.Success)
        {
            throw new InvalidOperationException("Failed to create instance!");
        }

        _alloter.Clear();

        return instance;
    }

    /// <summary>
    /// Create instance extension.
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException</exception>
    private T? CreateInstanceExtension<T>() where T : NativeExtension<Vk>
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

        Console.WriteLine($"[{messageSeverity}] {message}");

        return Vk.False;
    }
}
