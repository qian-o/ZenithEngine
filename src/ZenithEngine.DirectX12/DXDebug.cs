using System.Globalization;
using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDebug : GraphicsResource
{
    private static readonly PfnMessageFunc pfnMessage;

    private readonly uint callbackCookie;

    static DXDebug()
    {
        pfnMessage = new(MessageCallback);
    }

    public DXDebug(GraphicsContext context) : base(context)
    {
        Context.Device.QueryInterface(out ComPtr<ID3D12InfoQueue1> infoQueue).ThrowIfError();

        infoQueue.RegisterMessageCallback(pfnMessage,
                                          MessageCallbackFlags.FlagNone,
                                          null,
                                          ref callbackCookie).ThrowIfError();

        infoQueue.Dispose();
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        Context.Device.QueryInterface(out ComPtr<ID3D12InfoQueue1> infoQueue).ThrowIfError();

        infoQueue.UnregisterMessageCallback(callbackCookie);

        infoQueue.Dispose();
    }

    private static void MessageCallback(MessageCategory category,
                                        MessageSeverity severity,
                                        MessageID messageID,
                                        byte* pDescription,
                                        void* context)
    {
        string message = Utils.PtrToStringUTF8((nint)pDescription);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{severity}] [{category}]");

        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"MessageID: {messageID}");

        foreach (string str in strings)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{str}");
        }

        PrintMessage(stringBuilder.ToString(), severity switch
        {
            MessageSeverity.Corruption => ConsoleColor.DarkRed,
            MessageSeverity.Error => ConsoleColor.Red,
            MessageSeverity.Warning => ConsoleColor.Yellow,
            MessageSeverity.Info => ConsoleColor.Blue,
            MessageSeverity.Message => ConsoleColor.DarkGray,
            _ => Console.ForegroundColor
        });
    }

    private static void PrintMessage(string message, ConsoleColor color)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.ForegroundColor = color;

            Console.WriteLine(message);

            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
