namespace Graphics.Windowing.Interactivity;

public class PropertyEventArgs<T>(T value) : EventArgs
{
    public T Value { get; } = value;
}
