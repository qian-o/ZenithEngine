namespace Graphics.Windowing.Interactivity;

public class ValueEventArgs<T>(T value) : EventArgs
{
    public T Value { get; } = value;
}
