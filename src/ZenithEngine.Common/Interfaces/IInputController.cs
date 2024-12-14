using Silk.NET.Maths;
using ZenithEngine.Common.Events;

namespace ZenithEngine.Common.Interfaces;

public interface IInputController
{
    event EventHandler<KeyEventArgs>? KeyUp;

    event EventHandler<KeyEventArgs>? KeyDown;

    event EventHandler<ValueEventArgs<char>>? KeyChar;

    event EventHandler<MouseButtonEventArgs>? Click;

    event EventHandler<MouseButtonEventArgs>? MouseUp;

    event EventHandler<MouseButtonEventArgs>? MouseDown;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;
}
