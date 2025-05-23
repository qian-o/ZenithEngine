using Silk.NET.Maths;
using ZenithEngine.Common.Events;
using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Windowing;

internal partial class Window : IInput
{
    public Cursor Cursor { get => WindowUtils.GetCursor(); set => WindowUtils.SetCursor(value); }

    public event EventHandler<MouseButtonEventArgs>? Click;

    public event EventHandler<ValueEventArgs<char>>? KeyChar;

    public event EventHandler<KeyEventArgs>? KeyDown;

    public event EventHandler<KeyEventArgs>? KeyUp;

    public event EventHandler<MouseButtonEventArgs>? MouseDown;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    public event EventHandler<MouseButtonEventArgs>? MouseUp;

    public event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;
}