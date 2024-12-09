using Silk.NET.SDL;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

internal unsafe partial class Window : IWindow
{
    public SdlWindow* Handle;

    public SdlNativeWindow* NativeWindow;

    public void Show()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void Focus()
    {
        throw new NotImplementedException();
    }

    public void DoEvents()
    {
        throw new NotImplementedException();
    }

    public void DoUpdate()
    {
        throw new NotImplementedException();
    }

    public void DoRender()
    {
        throw new NotImplementedException();
    }

    private bool IsInitialized()
    {
        return Handle != null;
    }
}
