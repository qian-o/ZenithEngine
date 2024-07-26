using System.Numerics;
using Silk.NET.Input;

namespace Graphics.Core;

partial class SdlWindow
{
    public event EventHandler<MouseButtonEventArgs>? MouseDown;
    public event EventHandler<MouseButtonEventArgs>? MouseUp;
    public event EventHandler<MouseButtonEventArgs>? MouseClick;
    public event EventHandler<MouseButtonEventArgs>? MouseDoubleClick;
    public event EventHandler<MouseMoveEventArgs>? MouseMove;
    public event EventHandler<MouseWheelEventArgs>? MouseWheel;

    public Vector2 MousePositionByWindow => _mouse.Position;

    public Vector2 MousePositionByScreen => Position + MousePositionByWindow;

    public bool IsButtonPressed(MouseButton mouseButton)
    {
        return _mouse.IsButtonPressed(mouseButton);
    }

    private void AssemblyMouseEvent()
    {
        _mouse.MouseDown += (m, b) =>
        {
            MouseDown?.Invoke(this, new MouseButtonEventArgs(b, MousePositionByWindow));
        };

        _mouse.MouseUp += (m, b) =>
        {
            MouseUp?.Invoke(this, new MouseButtonEventArgs(b, MousePositionByWindow));
        };

        _mouse.Click += (m, b, p) =>
        {
            MouseClick?.Invoke(this, new MouseButtonEventArgs(b, p));
        };

        _mouse.DoubleClick += (m, b, p) =>
        {
            MouseDoubleClick?.Invoke(this, new MouseButtonEventArgs(b, p));
        };

        _mouse.MouseMove += (m, p) =>
        {
            MouseMove?.Invoke(this, new MouseMoveEventArgs(p, Position + p));
        };

        _mouse.Scroll += (m, w) =>
        {
            MouseWheel?.Invoke(this, new MouseWheelEventArgs(w));
        };
    }
}
