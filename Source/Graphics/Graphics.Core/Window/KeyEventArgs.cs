using Silk.NET.Input;

namespace Graphics.Core.Window;

public class KeyEventArgs(Key key, int scanCode) : EventArgs
{
    public Key Key { get; } = key;

    public int ScanCode { get; } = scanCode;
}
