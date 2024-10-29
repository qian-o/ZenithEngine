namespace Graphics.Windowing.Interactivity;

public class SizeChangedEventArgs(int width, int height) : EventArgs
{
    public int Width { get; } = width;

    public int Height { get; } = height;
}
