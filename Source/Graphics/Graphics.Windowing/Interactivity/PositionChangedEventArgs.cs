namespace Graphics.Windowing.Interactivity;

public class PositionChangedEventArgs(int x, int y) : EventArgs
{
    public int X { get; } = x;

    public int Y { get; } = y;
}
