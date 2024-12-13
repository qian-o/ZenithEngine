using Silk.NET.Maths;
using ZenithEngine.Windowing.Enums;
using ZenithEngine.Windowing.Events;

namespace ZenithEngine.Windowing.Interfaces;

public interface IWindowEvents
{
    event EventHandler<TimeEventArgs>? Update;

    event EventHandler<TimeEventArgs>? Render;

    event EventHandler<EventArgs>? Loaded;

    event EventHandler<EventArgs>? Unloaded;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    event EventHandler<ValueEventArgs<Vector2D<uint>>>? SizeChanged;

    event EventHandler<ValueEventArgs<WindowState>>? StateChanged;

    event EventHandler<KeyEventArgs>? KeyUp;

    event EventHandler<KeyEventArgs>? KeyDown;

    event EventHandler<ValueEventArgs<char>>? KeyChar;

    event EventHandler<MouseButtonEventArgs>? Click;

    event EventHandler<MouseButtonEventArgs>? MouseUp;

    event EventHandler<MouseButtonEventArgs>? MouseDown;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;
}
