using Silk.NET.Input;

namespace Graphics.Core;

public class MouseWheelEventArgs(ScrollWheel scrollWheel) : EventArgs
{
    public ScrollWheel ScrollWheel { get; } = scrollWheel;
}