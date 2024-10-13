using System.Globalization;
using System.Text;
using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Helpers;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Graphics.Vulkan;

public unsafe class VulkanDebug(Vk vk) : DisposableObject
{
    private static readonly bool _debugUtils;
    private static readonly bool _debugReport;

    private VkInstance? _instance;
    private ExtDebugUtils? _debugUtilsExt;
    private ExtDebugReport? _debugReportExt;
    private DebugUtilsMessengerEXT? _debugUtilsMessenger;
    private DebugReportCallbackEXT? _debugReportCallback;

    static VulkanDebug()
    {
        Vk vk = Vk.GetApi();

        uint extensionCount = 0;
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, null);

        ExtensionProperties[] availableExtensions = new ExtensionProperties[(int)extensionCount];
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, availableExtensions);

        bool debugUtils = false;
        bool debugReport = false;

        foreach (ExtensionProperties extension in availableExtensions)
        {
            if (Alloter.GetString(extension.ExtensionName) == ExtDebugUtils.ExtensionName)
            {
                debugUtils = true;
                break;
            }
            else if (Alloter.GetString(extension.ExtensionName) == ExtDebugReport.ExtensionName)
            {
                debugReport = true;
                break;
            }
        }

        _debugUtils = debugUtils;
        _debugReport = debugReport;

        vk.Dispose();
    }

    public static string ExtensionName => _debugUtils ? ExtDebugUtils.ExtensionName : _debugReport ? ExtDebugReport.ExtensionName : string.Empty;

    public void SetDebugMessageCallback(VkInstance instance)
    {
        Destroy();

        _instance = instance;

        if (_debugUtils)
        {
            _debugUtilsExt = CreateInstanceExtension<ExtDebugUtils>();

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
            _debugUtilsExt.CreateDebugUtilsMessenger(instance, &createInfo, null, &debugUtilsMessenger);

            _debugUtilsMessenger = debugUtilsMessenger;
        }

        if (_debugReport)
        {
            _debugReportExt = CreateInstanceExtension<ExtDebugReport>();

            DebugReportCallbackCreateInfoEXT createInfo = new()
            {
                SType = StructureType.DebugReportCallbackCreateInfoExt,
                Flags = DebugReportFlagsEXT.InformationBitExt
                        | DebugReportFlagsEXT.WarningBitExt
                        | DebugReportFlagsEXT.PerformanceWarningBitExt
                        | DebugReportFlagsEXT.ErrorBitExt
                        | DebugReportFlagsEXT.DebugBitExt,
                PfnCallback = (PfnDebugReportCallbackEXT)DebugMessageCallback
            };

            DebugReportCallbackEXT debugReportCallback;
            _debugReportExt.CreateDebugReportCallback(instance, &createInfo, null, &debugReportCallback);

            _debugReportCallback = debugReportCallback;
        }

        T CreateInstanceExtension<T>() where T : NativeExtension<Vk>
        {
            if (!vk.TryGetInstanceExtension(instance, out T ext))
            {
                throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
            }

            return ext;
        }
    }

    public void SetObjectName(VkDevice device, ref readonly DebugUtilsObjectNameInfoEXT nameInfo)
    {
        fixed (DebugUtilsObjectNameInfoEXT* pNameInfo = &nameInfo)
        {
            _debugUtilsExt?.SetDebugUtilsObjectName(device, pNameInfo).ThrowCode();
        }
    }

    protected override void Destroy()
    {
        if (_debugUtilsMessenger.HasValue)
        {
            _debugUtilsExt!.DestroyDebugUtilsMessenger(_instance!.Value, _debugUtilsMessenger.Value, null);
        }

        if (_debugReportCallback.HasValue)
        {
            _debugReportExt!.DestroyDebugReportCallback(_instance!.Value, _debugReportCallback.Value, null);
        }

        _debugUtilsExt?.Dispose();
        _debugReportExt?.Dispose();

        _instance = null;
        _debugUtilsExt = null;
        _debugReportExt = null;
        _debugUtilsMessenger = null;
        _debugReportCallback = null;
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

        PrintMessage(stringBuilder.ToString(), messageSeverity switch
        {
            DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt => ConsoleColor.DarkGray,
            DebugUtilsMessageSeverityFlagsEXT.InfoBitExt => ConsoleColor.Blue,
            DebugUtilsMessageSeverityFlagsEXT.WarningBitExt => ConsoleColor.Yellow,
            DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt => ConsoleColor.Red,
            _ => Console.ForegroundColor
        });

        return Vk.False;
    }


    private uint DebugMessageCallback(uint flags,
                                      DebugReportObjectTypeEXT objectType,
                                      ulong @object,
                                      nuint location,
                                      int messageCode,
                                      byte* pLayerPrefix,
                                      byte* pMessage,
                                      void* pUserData)
    {
        string message = Alloter.GetString(pMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{(DebugReportFlagsEXT)flags}] [{objectType}]");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Location: {location}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Message Code: {messageCode}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Layer Prefix: {Alloter.GetString(pLayerPrefix)}");
        foreach (string str in strings)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{str}");
        }

        PrintMessage(stringBuilder.ToString(), flags switch
        {
            (uint)DebugReportFlagsEXT.InformationBitExt => ConsoleColor.Blue,
            (uint)DebugReportFlagsEXT.WarningBitExt => ConsoleColor.Yellow,
            (uint)DebugReportFlagsEXT.PerformanceWarningBitExt => ConsoleColor.DarkYellow,
            (uint)DebugReportFlagsEXT.ErrorBitExt => ConsoleColor.Red,
            (uint)DebugReportFlagsEXT.DebugBitExt => ConsoleColor.DarkGray,
            _ => Console.ForegroundColor
        });

        return Vk.False;
    }

    private static void PrintMessage(string message, ConsoleColor color)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.ForegroundColor = color;
        }

        Console.WriteLine(message);

        if (OperatingSystem.IsWindows())
        {
            Console.ResetColor();
        }
    }
}
