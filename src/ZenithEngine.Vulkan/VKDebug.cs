using System.Globalization;
using System.Text;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDebug : GraphicsResource
{
    private static readonly bool debugUtilsSupported;
    private static readonly bool debugReportSupported;
    private static readonly bool debugMarkerSupported;
    private static readonly PfnDebugUtilsMessengerCallbackEXT pfnUtilsCallback;
    private static readonly PfnDebugReportCallbackEXT pfnReportCallback;

    private readonly ExtDebugUtils? utils;
    private readonly ExtDebugReport? report;
    private readonly ExtDebugMarker? marker;
    private readonly DebugUtilsMessengerEXT? utilsCallback;
    private readonly DebugReportCallbackEXT? reportCallback;

    static VKDebug()
    {
        using Vk vk = Vk.GetApi();

        uint extensionCount;
        vk.EnumerateInstanceExtensionProperties((string)null!,
                                                &extensionCount,
                                                null).ThrowIfError();

        ExtensionProperties[] extensions = new ExtensionProperties[extensionCount];
        vk.EnumerateInstanceExtensionProperties((string)null!,
                                                &extensionCount,
                                                extensions).ThrowIfError();

        foreach (ExtensionProperties extension in extensions)
        {
            string name = Utils.PtrToStringUTF8((nint)extension.ExtensionName);

            if (name is ExtDebugUtils.ExtensionName)
            {
                debugUtilsSupported = true;
            }
            else if (name is ExtDebugReport.ExtensionName)
            {
                debugReportSupported = true;
            }
            else if (name is ExtDebugMarker.ExtensionName)
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

        pfnUtilsCallback = (PfnDebugUtilsMessengerCallbackEXT)MessageCallback;
        pfnReportCallback = (PfnDebugReportCallbackEXT)MessageCallback;
    }

    public VKDebug(GraphicsContext context) : base(context)
    {
        utils = Context.Vk.TryGetExtension<ExtDebugUtils>(Context.Instance);
        report = Context.Vk.TryGetExtension<ExtDebugReport>(Context.Instance);
        marker = Context.Vk.TryGetExtension<ExtDebugMarker>(Context.Instance);

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
                PfnUserCallback = pfnUtilsCallback
            };

            DebugUtilsMessengerEXT messengerEXT;
            utils!.CreateDebugUtilsMessenger(Context.Instance,
                                             &createInfo,
                                             null,
                                             &messengerEXT).ThrowIfError();

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
                PfnCallback = pfnReportCallback
            };

            DebugReportCallbackEXT callbackEXT;
            report!.CreateDebugReportCallback(Context.Instance,
                                              &createInfo,
                                              null,
                                              &callbackEXT).ThrowIfError();

            reportCallback = callbackEXT;
        }
    }

    public static string[] ExtensionNames { get; }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

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
                PObjectName = allocator.AllocUTF8(name)
            };

            utils!.SetDebugUtilsObjectName(device, &nameInfo).ThrowIfError();
        }
        else if (debugMarkerSupported)
        {
            DebugMarkerObjectNameInfoEXT nameInfo = new()
            {
                SType = StructureType.DebugMarkerObjectNameInfoExt,
                ObjectType = (DebugReportObjectTypeEXT)type,
                Object = handle,
                PObjectName = allocator.AllocUTF8(name)
            };

            marker!.DebugMarkerSetObjectName(device, &nameInfo).ThrowIfError();
        }
    }

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        if (reportCallback.HasValue)
        {
            report!.DestroyDebugReportCallback(Context.Instance, reportCallback.Value, null);
        }

        if (utilsCallback.HasValue)
        {
            utils!.DestroyDebugUtilsMessenger(Context.Instance, utilsCallback.Value, null);
        }

        marker?.Dispose();
        report?.Dispose();
        utils?.Dispose();
    }

    private static uint MessageCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
                                        DebugUtilsMessageTypeFlagsEXT messageTypes,
                                        DebugUtilsMessengerCallbackDataEXT* pCallbackData,
                                        void* pUserData)
    {
        string message = Utils.PtrToStringUTF8((nint)pCallbackData->PMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                                 $"[{messageSeverity}] [{messageTypes}]");

        stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                                 $"Name: {Utils.PtrToStringUTF8((nint)pCallbackData->PMessageIdName)}");

        stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                                 $"Number: {pCallbackData->MessageIdNumber}");

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

    private static uint MessageCallback(uint flags,
                                        DebugReportObjectTypeEXT objectType,
                                        ulong @object,
                                        nuint location,
                                        int messageCode,
                                        byte* pLayerPrefix,
                                        byte* pMessage,
                                        void* pUserData)
    {
        string message = Utils.PtrToStringUTF8((nint)pMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                                 $"[{(DebugReportFlagsEXT)flags}] [{objectType}]");

        stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                                 $"Location: {location}");

        stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                                 $"Message Code: {messageCode}");

        stringBuilder.AppendLine(CultureInfo.InvariantCulture,
                                 $"Layer Prefix: {Utils.PtrToStringUTF8((nint)pLayerPrefix)}");

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
