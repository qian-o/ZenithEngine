namespace Graphics.Core;

partial class Window
{
    public event EventHandler<KeyEventArgs>? KeyDown;
    public event EventHandler<KeyEventArgs>? KeyUp;
    public event EventHandler<KeyCharEventArgs>? KeyChar;

    private void AssemblyKeyboardEvent()
    {
        _keyboard.KeyDown += (kb, k, i) =>
        {
            KeyDown?.Invoke(this, new KeyEventArgs(k, i));
        };

        _keyboard.KeyUp += (kb, k, i) =>
        {
            KeyUp?.Invoke(this, new KeyEventArgs(k, i));
        };

        _keyboard.KeyChar += (kb, c) =>
        {
            KeyChar?.Invoke(this, new KeyCharEventArgs(c));
        };
    }
}
