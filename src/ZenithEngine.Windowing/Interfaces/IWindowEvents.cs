﻿using Silk.NET.Maths;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;

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
}
