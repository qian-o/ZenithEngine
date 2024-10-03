using Silk.NET.Input;

namespace Graphics.Core.Window;

public class MouseWheelEventArgs(ScrollWheel scrollWheel) : EventArgs
{
    public ScrollWheel ScrollWheel { get; } = scrollWheel;
}