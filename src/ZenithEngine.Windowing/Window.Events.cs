using Silk.NET.Maths;
using ZenithEngine.Windowing.Enums;
using ZenithEngine.Windowing.Events;

namespace ZenithEngine.Windowing;

internal partial class Window
{
    public event EventHandler<EventArgs>? Loaded;

    public event EventHandler<TimeEventArgs>? Update;

    public event EventHandler<TimeEventArgs>? Render;

    public event EventHandler<ValueEventArgs<WindowState>>? StateChanged;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? SizeChanged;

    public event EventHandler<KeyEventArgs>? KeyDown;

    public event EventHandler<KeyEventArgs>? KeyUp;

    public event EventHandler<ValueEventArgs<char>>? KeyChar;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    public event EventHandler<MouseButtonEventArgs>? MouseDown;

    public event EventHandler<MouseButtonEventArgs>? MouseUp;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;

    public event EventHandler<MouseButtonEventArgs>? Click;

    public event EventHandler<MouseButtonEventArgs>? DoubleClick;

    public event EventHandler<EventArgs>? Unloaded;
}
