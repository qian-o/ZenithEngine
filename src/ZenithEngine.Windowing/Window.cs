using Silk.NET.Maths;
using ZenithEngine.Common.Interfaces;
using ZenithEngine.Windowing.Enums;
using ZenithEngine.Windowing.Events;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

internal partial class Window : IWindow
{
    public nint Handle { get; }

    public double Time { get; }

    public bool IsFocused { get; }

    public float DpiScale { get; }

    public ISurface Surface { get; }

    public string Title { get; set; }

    public WindowState State { get; set; }

    public WindowBorder Border { get; set; }

    public Vector2D<int> Position { get; set; }

    public Vector2D<int> Size { get; set; }

    public Vector2D<int> MinimumSize { get; set; }

    public Vector2D<int> MaximumSize { get; set; }

    public bool IsVisible { get; set; }

    public bool TopMost { get; set; }

    public bool ShowInTaskbar { get; set; }

    public float Opacity { get; set; }

    public double UpdatePerSecond { get; set; }

    public double RenderPerSecond { get; set; }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void DoEvents()
    {
        throw new NotImplementedException();
    }

    public void DoRender()
    {
        throw new NotImplementedException();
    }

    public void DoUpdate()
    {
        throw new NotImplementedException();
    }

    public void Focus()
    {
        throw new NotImplementedException();
    }

    public void Show()
    {
        throw new NotImplementedException();
    }
}
