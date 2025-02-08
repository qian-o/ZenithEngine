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

    public ComPtr<ID3D12InfoQueue1> InfoQueue1;

    static DXDebug()
    {
        pfnMessage = new(MessageCallback);
    }

    public DXDebug(GraphicsContext context) : base(context)
    {
        Context.Device.QueryInterface(out InfoQueue1).ThrowIfError();

        uint callbackCookie;
        InfoQueue1.RegisterMessageCallback(pfnMessage,
                                           MessageCallbackFlags.FlagNone,
                                           null,
                                           &callbackCookie).ThrowIfError();
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void DebugName(string name)
    {
    }

    protected override void Destroy()
    {
        InfoQueue1.Dispose();
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
