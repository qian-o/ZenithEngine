using Silk.NET.Maths;
using ZenithEngine.Windowing.Enums;

namespace ZenithEngine.Windowing.Events;

public class MouseButtonEventArgs(MouseButton button, Vector2D<int> position, uint clicks) : EventArgs
{
    public MouseButton Button { get; } = button;

    public Vector2D<int> Position { get; } = position;

    public uint Clicks { get; } = clicks;
}

