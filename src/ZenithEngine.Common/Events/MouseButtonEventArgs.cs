using Silk.NET.Maths;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Events;

public class MouseButtonEventArgs(MouseButton button, Vector2D<int> position, uint clicks) : EventArgs
{
    public MouseButton Button { get; } = button;

    public Vector2D<int> Position { get; } = position;

    public uint Clicks { get; } = clicks;
}
