using Silk.NET.Maths;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Windowing;

internal partial class Window : IWindowEvents
{
    public event EventHandler<TimeEventArgs>? Update;

    public event EventHandler<TimeEventArgs>? Render;

    public event EventHandler<EventArgs>? Loaded;

    public event EventHandler<EventArgs>? Unloaded;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    public event EventHandler<ValueEventArgs<Vector2D<uint>>>? SizeChanged;

    public event EventHandler<ValueEventArgs<WindowState>>? StateChanged;
}
