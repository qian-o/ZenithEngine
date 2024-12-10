using Silk.NET.Core.Contexts;
using Silk.NET.SDL;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Windowing;

internal readonly unsafe struct Surface : ISurface
{
    public Surface(SdlWindow* handle)
    {
        SdlNativeWindow nativeWindow = new(WindowUtils.Sdl, handle);

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
            throw new PlatformNotSupportedException("The platform is not supported.");
        }
    }

    public SurfaceType SurfaceType { get; }

    public nint[] Handles { get; }
}
