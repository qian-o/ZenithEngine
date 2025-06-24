using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Events;

public class GamepadButtonEventArgs(int gamepadId, GamepadButton button) : EventArgs
{
    public int GamepadId { get; } = gamepadId;

    public GamepadButton Button { get; } = button;
}