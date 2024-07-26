namespace Graphics.Core;

public class FocusChangedEventArgs(bool isFocused) : EventArgs
{
    public bool IsFocused { get; } = isFocused;
}