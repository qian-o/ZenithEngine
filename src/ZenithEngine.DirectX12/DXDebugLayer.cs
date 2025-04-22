using System.Globalization;
using System.Text;
using Silk.NET.Direct3D12;
using ZenithEngine.Common;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.DirectX12;

internal unsafe class DXDebugLayer : GraphicsResource
{
    private readonly uint callbackCookie;

    public DXDebugLayer(GraphicsContext context) : base(context)
    {
        Context.InfoQueue1.RegisterMessageCallback(new(MessageCallback),
                                                   MessageCallbackFlags.FlagNone,
                                                   null,
                                                   ref callbackCookie).ThrowIfError();
    }

    private new DXGraphicsContext Context => (DXGraphicsContext)base.Context;

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        Context.InfoQueue1.UnregisterMessageCallback(callbackCookie).ThrowIfError();
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

        if (OperatingSystem.IsWindows())
        {
            Console.ForegroundColor = severity switch
            {
                MessageSeverity.Corruption => ConsoleColor.DarkRed,
                MessageSeverity.Error => ConsoleColor.Red,
                MessageSeverity.Warning => ConsoleColor.Yellow,
                MessageSeverity.Info => ConsoleColor.Blue,
                MessageSeverity.Message => ConsoleColor.DarkGray,
                _ => Console.ForegroundColor
            };

            Console.WriteLine(stringBuilder.ToString());

            Console.ResetColor();
        }
        else
        {
            Console.WriteLine(stringBuilder.ToString());
        }
    }
}
