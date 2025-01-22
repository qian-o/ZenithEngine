using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Events;

public class KeyEventArgs(Key key, KeyModifiers modifiers) : EventArgs
{
    public Key Key { get; } = key;

    public KeyModifiers Modifiers { get; } = modifiers;
}
