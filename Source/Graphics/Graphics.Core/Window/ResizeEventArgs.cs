namespace Graphics.Core.Window;

public class ResizeEventArgs(uint width, uint height) : EventArgs
{
    public uint Width { get; } = width;

    public uint Height { get; } = height;
}
