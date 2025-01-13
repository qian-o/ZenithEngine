using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.SDL;
using ZenithEngine.Common;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Windowing;

internal readonly unsafe struct Surface : ISurface
{
    private readonly SdlWindow* window;

    public Surface(SdlWindow* handle)
    {
        window = handle;

        SdlNativeWindow nativeWindow = new(WindowUtils.Sdl, window);

        if (nativeWindow.Kind.HasFlag(NativeWindowFlags.Win32))
        {
            SurfaceType = SurfaceType.Win32;
            Handles = [nativeWindow.Win32!.Value.Hwnd];
        }
        else if (nativeWindow.Kind.HasFlag(NativeWindowFlags.Wayland))
        {
            SurfaceType = SurfaceType.Wayland;
            Handles = [nativeWindow.Wayland!.Value.Display, nativeWindow.Wayland!.Value.Surface];
        }
        else if (nativeWindow.Kind.HasFlag(NativeWindowFlags.X11))
        {
            SurfaceType = SurfaceType.Xlib;
            Handles = [nativeWindow.X11!.Value.Display, (nint)nativeWindow.X11!.Value.Window];
        }
        else if (nativeWindow.Kind.HasFlag(NativeWindowFlags.Cocoa))
        {
            SurfaceType = SurfaceType.MacOS;
            Handles = [nativeWindow.Cocoa!.Value];
        }
        else
        {
            throw new NotSupportedException(ExceptionHelpers.NotSupported(nativeWindow.Kind));
        }
    }

    public SurfaceType SurfaceType { get; }

    public nint[] Handles { get; }

    public Vector2D<uint> GetSize()
    {
        Vector2D<int> size;
        WindowUtils.Sdl.GetWindowSize(window, &size.X, &size.Y);

        return size.As<uint>();
    }
}
