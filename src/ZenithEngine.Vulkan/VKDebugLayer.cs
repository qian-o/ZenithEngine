using System.Globalization;
using System.Text;
using Silk.NET.Vulkan;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan;

internal unsafe class VKDebugLayer : GraphicsResource
{
    private readonly DebugUtilsMessengerEXT messenger;

    public VKDebugLayer(GraphicsContext context) : base(context)
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
                          | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
            PfnUserCallback = new(MessageCallback)
        };

        Context.ExtDebugUtils!.CreateDebugUtilsMessenger(Context.Instance,
                                                         &createInfo,
                                                         null,
                                                         out messenger).ThrowIfError();
    }

    private new VKGraphicsContext Context => (VKGraphicsContext)base.Context;

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        Context.ExtDebugUtils!.DestroyDebugUtilsMessenger(Context.Instance, messenger, null);
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

        if (OperatingSystem.IsWindows())
        {
            Console.ForegroundColor = messageSeverity switch
            {
                DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt => ConsoleColor.DarkGray,
                DebugUtilsMessageSeverityFlagsEXT.InfoBitExt => ConsoleColor.Blue,
                DebugUtilsMessageSeverityFlagsEXT.WarningBitExt => ConsoleColor.Yellow,
                DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };

            Console.WriteLine(stringBuilder.ToString());

            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(stringBuilder.ToString());
        }

        return Vk.False;
    }
}
