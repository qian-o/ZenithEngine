using System.Numerics;

namespace Graphics.Core;

public class MouseMoveEventArgs(Vector2 positionByWindow, Vector2 positionByScreen) : EventArgs
{
    public Vector2 PositionByWindow { get; } = positionByWindow;

    public Vector2 PositionByScreen { get; } = positionByScreen;
}