namespace Graphics.Core;

public class KeyCharEventArgs(char keyChar) : EventArgs
{
    public char KeyChar { get; } = keyChar;
}
