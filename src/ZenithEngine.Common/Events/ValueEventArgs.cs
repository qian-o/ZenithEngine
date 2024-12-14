namespace ZenithEngine.Common.Events;

public class ValueEventArgs<T>(T value) : EventArgs
{
    public T Value { get; } = value;
}
