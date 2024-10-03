namespace Graphics.Core.Window;

public class KeyCharEventArgs(char keyChar) : EventArgs
{
    public char KeyChar { get; } = keyChar;
}
