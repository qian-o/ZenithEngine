using Graphics.Windowing.Enums;

namespace Graphics.Windowing.Interactivity;

public class StateChangedEventArgs(WindowState state) : EventArgs
{
    public WindowState State { get; } = state;
}
