﻿using Silk.NET.Maths;
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

    public event EventHandler<KeyEventArgs>? KeyUp;

    public event EventHandler<KeyEventArgs>? KeyDown;

    public event EventHandler<ValueEventArgs<char>>? KeyChar;

    public event EventHandler<MouseButtonEventArgs>? Click;

    public event EventHandler<MouseButtonEventArgs>? MouseUp;

    public event EventHandler<MouseButtonEventArgs>? MouseDown;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;
}
