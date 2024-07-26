using Silk.NET.Input;

namespace Graphics.Core;

public class KeyEventArgs(Key key, int scanCode) : EventArgs
{
    public Key Key { get; } = key;

    public int ScanCode { get; } = scanCode;
}
