using Silk.NET.SDL;

namespace ZenithEngine.Windowing;

public static unsafe class WindowManager
{
    public static Sdl Sdl { get; } = Sdl.GetApi();
}
