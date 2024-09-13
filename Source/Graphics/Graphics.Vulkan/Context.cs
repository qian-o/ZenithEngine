using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
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
        Vk = Vk.GetApi();

        // Create instance
        Instance = CreateInstance();

        // Load instance extensions
        DebugUtilsExt = Debugging ? CreateInstanceExtension<ExtDebugUtils>() : null;
        SurfaceExt = CreateInstanceExtension<KhrSurface>();

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
            DebugUtilsExt!.CreateDebugUtilsMessenger(Instance, &createInfo, null, &debugUtilsMessenger)
                           .ThrowCode("Failed to create debug messenger!");

            DebugUtilsMessenger = debugUtilsMessenger;
        }
    }

    internal Alloter Alloter { get; } = new();

    internal Vk Vk { get; }

    internal Instance Instance { get; }

    internal ExtDebugUtils? DebugUtilsExt { get; }

    internal KhrSurface SurfaceExt { get; }

    internal DebugUtilsMessengerEXT? DebugUtilsMessenger { get; }

    public static bool Debugging { get; }

    protected override void Destroy()
    {
        SurfaceExt.Dispose();

        DebugUtilsExt?.DestroyDebugUtilsMessenger(Instance, DebugUtilsMessenger!.Value, null);
        DebugUtilsExt?.Dispose();

        Vk.DestroyInstance(Instance, null);
        Vk.Dispose();

        Alloter.Dispose();
    }

    /// <summary>
    /// Check validation layer support.
    /// </summary>
    /// <returns></returns>
    private bool CheckValidationLayerSupport()
    {
        uint layerCount = 0;
        Vk.EnumerateInstanceLayerProperties(&layerCount, null);

        LayerProperties[] availableLayers = new LayerProperties[(int)layerCount];
        Vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

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
            PApplicationName = Alloter.Allocate("Graphics"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = Alloter.Allocate("Graphics"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version13
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
            createInfo.PpEnabledLayerNames = Alloter.Allocate([ValidationLayerName]);
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

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = Alloter.Allocate(extensions);

        Instance instance;
        Vk.CreateInstance(&createInfo, null, &instance).ThrowCode("Failed to create instance!");

        Alloter.Clear();

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
        if (!Vk.TryGetInstanceExtension(Instance, out T ext))
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
}
