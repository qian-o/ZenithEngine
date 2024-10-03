namespace Graphics.Core.Window;

public class MoveEventArgs(int x, int y) : EventArgs
{
    public int X { get; } = x;

    public int Y { get; } = y;
}
