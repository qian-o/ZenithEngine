using System.Numerics;
using Silk.NET.Input;

namespace Graphics.Core.Window;

public class MouseButtonEventArgs(MouseButton mouseButton, Vector2 position) : EventArgs
{
    public MouseButton MouseButton { get; } = mouseButton;

    public Vector2 Position { get; } = position;
}
