using System.Numerics;

namespace Graphics.Core.Window;

public class MouseMoveEventArgs(Vector2 position) : EventArgs
{
    public Vector2 Position { get; } = position;
}