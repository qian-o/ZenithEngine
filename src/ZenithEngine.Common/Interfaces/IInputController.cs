using Silk.NET.Maths;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;

namespace ZenithEngine.Common.Interfaces;

public interface IInputController
{
    Cursor Cursor { get; set; }

    event EventHandler<MouseButtonEventArgs>? Click;

    event EventHandler<ValueEventArgs<char>>? KeyChar;

    event EventHandler<KeyEventArgs>? KeyDown;

    event EventHandler<KeyEventArgs>? KeyUp;

    event EventHandler<MouseButtonEventArgs>? MouseDown;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    event EventHandler<MouseButtonEventArgs>? MouseUp;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;
}
