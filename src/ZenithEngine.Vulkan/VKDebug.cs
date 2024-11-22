using System.Globalization;
using System.Text;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using ZenithEngine.Common;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDebug : DisposableObject
{
    private static readonly bool debugUtilsSupported;
    private static readonly bool debugReportSupported;
    private static readonly bool debugMarkerSupported;

    private readonly VkInstance instance;
    private readonly ExtDebugUtils? utils;
    private readonly ExtDebugReport? report;
    private readonly ExtDebugMarker? marker;
    private readonly DebugUtilsMessengerEXT? utilsCallback;
    private readonly DebugReportCallbackEXT? reportCallback;

    static VKDebug()
    {
        using Vk vk = Vk.GetApi();

        uint extensionCount = 0;
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, null).ThrowIfError();

        ExtensionProperties[] extensions = new ExtensionProperties[extensionCount];
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, extensions).ThrowIfError();

        foreach (ExtensionProperties extension in extensions)
        {
            string name = Utils.PtrToStringAnsi((nint)extension.ExtensionName);

            if (name == ExtDebugUtils.ExtensionName)
            {
                debugUtilsSupported = true;
            }
            else if (name == ExtDebugReport.ExtensionName)
            {
                debugReportSupported = true;
            }
            else if (name == ExtDebugMarker.ExtensionName)
            {
                debugMarkerSupported = true;
            }
        }

        if (debugUtilsSupported)
        {
            ExtensionNames = [ExtDebugUtils.ExtensionName];
        }
        else
        {
            ExtensionNames = [];

            if (debugReportSupported)
            {
                ExtensionNames = [ExtDebugReport.ExtensionName];
            }

            if (debugMarkerSupported)
            {
                ExtensionNames = [.. ExtensionNames, ExtDebugMarker.ExtensionName];
            }
        }
    }

    public VKDebug(VKGraphicsContext context)
    {
        instance = context.Instance;

        utils = context.Vk.TryGetExtension<ExtDebugUtils>(instance);
        report = context.Vk.TryGetExtension<ExtDebugReport>(instance);
        marker = context.Vk.TryGetExtension<ExtDebugMarker>(instance);

        if (debugUtilsSupported)
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
                PfnUserCallback = (PfnDebugUtilsMessengerCallbackEXT)MessageCallback
            };

            DebugUtilsMessengerEXT messengerEXT;
            utils!.CreateDebugUtilsMessenger(instance, &createInfo, null, &messengerEXT);

            utilsCallback = messengerEXT;
        }
        else if (debugReportSupported)
        {
            DebugReportCallbackCreateInfoEXT createInfo = new()
            {
                SType = StructureType.DebugReportCallbackCreateInfoExt,
                Flags = DebugReportFlagsEXT.InformationBitExt
                        | DebugReportFlagsEXT.WarningBitExt
                        | DebugReportFlagsEXT.PerformanceWarningBitExt
                        | DebugReportFlagsEXT.ErrorBitExt
                        | DebugReportFlagsEXT.DebugBitExt,
                PfnCallback = (PfnDebugReportCallbackEXT)MessageCallback
            };

            DebugReportCallbackEXT callbackEXT;
            report!.CreateDebugReportCallback(instance, &createInfo, null, &callbackEXT);

            reportCallback = callbackEXT;
        }
    }

    public static string[] ExtensionNames { get; }

    public void SetObjectName(VkDevice device,
                              ObjectType type,
                              ulong handle,
                              string name)
    {
        using MemoryAllocator allocator = new();

        if (debugUtilsSupported)
        {
            DebugUtilsObjectNameInfoEXT nameInfo = new()
            {
                SType = StructureType.DebugUtilsObjectNameInfoExt,
                ObjectType = type,
                ObjectHandle = handle,
                PObjectName = (byte*)allocator.AllocAnsi(name)
            };

            utils!.SetDebugUtilsObjectName(device, &nameInfo);
        }
        else if (debugMarkerSupported)
        {
            DebugMarkerObjectNameInfoEXT nameInfo = new()
            {
                SType = StructureType.DebugMarkerObjectNameInfoExt,
                ObjectType = (DebugReportObjectTypeEXT)type,
                Object = handle,
                PObjectName = (byte*)allocator.AllocAnsi(name)
            };

            marker!.DebugMarkerSetObjectName(device, &nameInfo);
        }
    }

    protected override void Destroy()
    {
        if (reportCallback.HasValue)
        {
            report!.DestroyDebugReportCallback(instance, reportCallback.Value, null);
        }

        if (utilsCallback.HasValue)
        {
            utils!.DestroyDebugUtilsMessenger(instance, utilsCallback.Value, null);
        }

        marker?.Dispose();
        report?.Dispose();
        utils?.Dispose();
    }

    private uint MessageCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
                                 DebugUtilsMessageTypeFlagsEXT messageTypes,
                                 DebugUtilsMessengerCallbackDataEXT* pCallbackData,
                                 void* pUserData)
    {
        string message = Utils.PtrToStringAnsi((nint)pCallbackData->PMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{messageSeverity}] [{messageTypes}]");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {Utils.PtrToStringAnsi((nint)pCallbackData->PMessageIdName)}");
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

    private uint MessageCallback(uint flags,
                                 DebugReportObjectTypeEXT objectType,
                                 ulong @object,
                                 nuint location,
                                 int messageCode,
                                 byte* pLayerPrefix,
                                 byte* pMessage,
                                 void* pUserData)
    {
        string message = Utils.PtrToStringAnsi((nint)pMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{(DebugReportFlagsEXT)flags}] [{objectType}]");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Location: {location}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Message Code: {messageCode}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Layer Prefix: {Utils.PtrToStringAnsi((nint)pLayerPrefix)}");
        foreach (string str in strings)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{str}");
        }

        PrintMessage(stringBuilder.ToString(), (DebugReportFlagsEXT)flags switch
        {
            DebugReportFlagsEXT.InformationBitExt => ConsoleColor.Blue,
            DebugReportFlagsEXT.WarningBitExt => ConsoleColor.Yellow,
            DebugReportFlagsEXT.PerformanceWarningBitExt => ConsoleColor.DarkYellow,
            DebugReportFlagsEXT.ErrorBitExt => ConsoleColor.Red,
            DebugReportFlagsEXT.DebugBitExt => ConsoleColor.DarkGray,
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
